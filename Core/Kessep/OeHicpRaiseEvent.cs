// Program: OE_HICP_RAISE_EVENT, ID: 371846355, model: 746.
// Short name: SWE01828
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_HICP_RAISE_EVENT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Written by Raju     : Dec 19 1996
/// This action block raises the event(s) for Infrastructure for following prads
/// 	HICP
/// </para>
/// </summary>
[Serializable]
public partial class OeHicpRaiseEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_RAISE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpRaiseEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpRaiseEvent.
  /// </summary>
  public OeHicpRaiseEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 12/19/96	raju	initial creation
    // 01/08/97	raju	initiating state code made 2 chars OS/KS
    // 08/12/99        D.Lowry PR719 change read for case unit to include AR or 
    // AP.
    // 11/09/99	D.Lowry PR79596.  Add dates to case unit read.
    // --------------------------------------------
    // 10/30/00      P.Phinney I00105957  Prevent multiple HIST records.
    // ------------------------------------------------
    // Assigning global infrastructure attribute values
    // ------------------------------------------------
    MoveInfrastructure(import.Infrastructure, local.Infrastructure);
    local.Infrastructure.DenormNumeric12 =
      import.HolderHealthInsuranceCoverage.Identifier;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    if (ReadCsePerson1())
    {
      local.Infrastructure.CsePersonNumber = entities.Holder.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadCsePerson2())
    {
      local.Infrastructure.DenormText12 = entities.Insured.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      // 10/30/00      P.Phinney I00105957  Prevent multiple HIST records.
      // Default before READ - IF read successful it will override
      local.Infrastructure.InitiatingStateCode = "KS";

      if (ReadInterstateRequest())
      {
        // -----------------------------------------------------------------------
        // reason codes are not unique but the combination of reason
        // code, initiating state code is unique and is used to get the
        // correct event detail record.
        // -----------------------------------------------------------------------
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
      }

      // -----------------------------------------------------------------------
      // Raju 01/02/1997 : 1000 hrs CST
      //   - Refer Jack's note dated 01/02/1997
      //     subject : Case Closure and Reopen,
      //               Case Unit Deactivation and
      //               Reactivation
      // Conclusion drawn from note :
      // The check for raising events only for active case units is to
      // be removed from all raise event cabs.
      // The event processor will handle this.
      // -----------------------------------------------------------------------
      // *** August 12, 1999  David Lowry
      // Problem report 719.  The read of case unit should be for the AR or the 
      // AP person.
      // *** November 09 1999 David Lowry
      // PR79596.  The date has to be used to get the correct case unit.
      if (ReadCaseUnit())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      }
      else
      {
        ExitState = "CASE_UNIT_NF";

        return;
      }

      // 10/30/00      P.Phinney I00105957  Prevent multiple HIST records.
      // Change to use only ONE write to Infrastructure
      UseSpCabCreateInfrastructure();
    }
    else
    {
      ExitState = "CASE_NF";
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, export.Infrastructure);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.Holder.Number);
        db.SetNullableString(command, "cspNoChild", entities.Insured.Number);
        db.SetNullableDate(
          command, "closureDate",
          import.HolderHealthInsuranceCoverage.PolicyExpirationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Holder.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.HolderCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Holder.Number = db.GetString(reader, 0);
        entities.Holder.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Insured.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Insured.Number);
      },
      (db, reader) =>
      {
        entities.Insured.Number = db.GetString(reader, 0);
        entities.Insured.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of HolderHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("holderHealthInsuranceCoverage")]
    public HealthInsuranceCoverage HolderHealthInsuranceCoverage
    {
      get => holderHealthInsuranceCoverage ??= new();
      set => holderHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of HolderCsePerson.
    /// </summary>
    [JsonPropertyName("holderCsePerson")]
    public CsePerson HolderCsePerson
    {
      get => holderCsePerson ??= new();
      set => holderCsePerson = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private HealthInsuranceCoverage holderHealthInsuranceCoverage;
    private CsePerson insured;
    private Case1 case1;
    private CsePerson holderCsePerson;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Holder.
    /// </summary>
    [JsonPropertyName("holder")]
    public CsePerson Holder
    {
      get => holder ??= new();
      set => holder = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson insured;
    private InterstateRequest interstateRequest;
    private CsePerson holder;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
