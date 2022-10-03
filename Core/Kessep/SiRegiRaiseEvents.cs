// Program: SI_REGI_RAISE_EVENTS, ID: 371727795, model: 746.
// Short name: SWE01923
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_REGI_RAISE_EVENTS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRegiRaiseEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_RAISE_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiRaiseEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiRaiseEvents.
  /// </summary>
  public SiRegiRaiseEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ??/??/??  ?????????             Initial Development
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    local.Curent.Date = Now().Date;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      local.TextWorkArea.Text4 =
        NumberToString(entities.Case1.OfficeIdentifier.GetValueOrDefault(), 4);
    }
    else
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    // ***	Begin Event insertion	***
    if (AsChar(import.FromIapi.Flag) == 'Y')
    {
      local.Infrastructure.ReasonCode = "CASOPNINTSTATE";
    }
    else if (AsChar(import.FromInrd.Flag) == 'Y')
    {
      local.Infrastructure.ReasonCode = "CASOPNINFOREQ";
    }
    else if (AsChar(import.FromPar1.Flag) == 'Y')
    {
      local.Infrastructure.ReasonCode = "CASOPNPARFRL";
    }
    else
    {
      local.Infrastructure.ReasonCode = "CASOPNOTHR";
    }

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

    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.EventId = 5;
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.CaseNumber = import.Case1.Number;
    local.Infrastructure.UserId = "REGI";
    local.Infrastructure.ReferenceDate = entities.Case1.StatusDate;
    local.Infrastructure.SituationNumber = 0;

    if (import.Ap.Count > 0)
    {
      foreach(var item in ReadCaseUnit())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.TextWorkArea.Text30 = "Case Opened :";
        local.TextWorkArea.Text10 = "; Office :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text8 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text8 + local.TextWorkArea.Text10 + local
          .TextWorkArea.Text4;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }
    else
    {
      local.TextWorkArea.Text30 = "Case Opened :";
      local.TextWorkArea.Text10 = "; Office :";
      local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
      local.TextWorkArea.Text8 = UseCabConvertDate2String();
      local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
        .TextWorkArea.Text8 + local.TextWorkArea.Text10 + local
        .TextWorkArea.Text4;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // ***	2nd Event insertion	***
      local.Infrastructure.ReasonCode = "CASOPNWOCAU";
      local.Infrastructure.Detail =
        TrimEnd("Case opened without any Case Units:") + local
        .TextWorkArea.Text8 + local.TextWorkArea.Text10 + local
        .TextWorkArea.Text4;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
      }
    }

    // ***	End Event insertion	***
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
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
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 4);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 5);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Case1.Note = db.GetNullableString(reader, 7);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseUnit.CasNo = db.GetString(reader, 6);
        entities.CaseUnit.Populated = true;

        return true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromInrd.
    /// </summary>
    [JsonPropertyName("fromInrd")]
    public Common FromInrd
    {
      get => fromInrd ??= new();
      set => fromInrd = value;
    }

    private Common ap;
    private Case1 case1;
    private Common fromIapi;
    private Common fromPar1;
    private Common fromInrd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of Curent.
    /// </summary>
    [JsonPropertyName("curent")]
    public DateWorkArea Curent
    {
      get => curent ??= new();
      set => curent = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Infrastructure infrastructure;
    private DateWorkArea curent;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CaseUnit caseUnit;
  }
#endregion
}
