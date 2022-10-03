// Program: SI_CHDS_RAISE_EVENT, ID: 371757641, model: 746.
// Short name: SWE01830
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHDS_RAISE_EVENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Written by Raju     : Dec 23 1996
/// This action block raises the event(s) for Infrastructure for following prads
/// 	CHDS
/// </para>
/// </summary>
[Serializable]
public partial class SiChdsRaiseEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHDS_RAISE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChdsRaiseEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChdsRaiseEvent.
  /// </summary>
  public SiChdsRaiseEvent(IContext context, Import import, Export export):
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
    // 12/23/96	raju	initial creation
    // 01/08/97	raju	initiating state code
    // 			made 2 chars OS/KS
    // --------------------------------------------
    // 05/26/99 W.Campbell     Replaced zd exit states.
    // -----------------------------------------------
    // 06/22/99 W.Campbell     Modified the properties
    //                         of a READ statement to
    //                         Select Only.
    // ---------------------------------------------------------
    // ---------------------------------------------
    // This is a copy of oe_cab_raise_event
    //   except that the read case unit in the
    //   oe_cab_raise_event is related to an ap
    //   and here it is for the child
    // ---------------------------------------------
    // --------------------------------------------
    // Assigning global infrastructure attribute
    //   values
    // --------------------------------------------
    MoveInfrastructure2(import.Infrastructure, local.Infrastructure);
    local.Infrastructure.CsePersonNumber = import.Ch.Number;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCsePerson())
    {
      // -----------------------------------------------
      // 05/26/99 W.Campbell -  Replaced zd exit states.
      // -----------------------------------------------
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Raju 01/02/1997 : 1000 hrs CST
    //   - Refer Jack's note dated 01/02/1997
    //     subject : Case Closure and Reopen,
    //               Case Unit Deactivation and
    //               Reactivation
    // Conclusion drawn from note :
    // The check for raising events only for active
    //   case units is to be removed from all
    //   raise event cabs.
    // The event processor will handle this.
    // ---------------------------------------------
    foreach(var item in ReadCaseUnit())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        // ---------------------------------------------
        // This is another important piece of code.
        //   - reason codes are not unique but the
        //     combination of reason code , initiating
        //     state code is unique and is used to get
        //     the correct event detail record.
        // ---------------------------------------------
        if (ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }

      UseSpCabCreateInfrastructure();
      MoveInfrastructure1(local.Infrastructure, export.Infrastructure);
    }
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
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

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ch.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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

    private CsePerson ch;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CaseRole Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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

    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private CaseRole zdel;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
