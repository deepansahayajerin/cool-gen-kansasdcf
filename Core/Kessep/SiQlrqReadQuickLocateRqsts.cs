// Program: SI_QLRQ_READ_QUICK_LOCATE_RQSTS, ID: 372393249, model: 746.
// Short name: SWE01188
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
/// A program: SI_QLRQ_READ_QUICK_LOCATE_RQSTS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB will list all the Quick Locate Requests for the AP.
/// </para>
/// </summary>
[Serializable]
public partial class SiQlrqReadQuickLocateRqsts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QLRQ_READ_QUICK_LOCATE_RQSTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQlrqReadQuickLocateRqsts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQlrqReadQuickLocateRqsts.
  /// </summary>
  public SiQlrqReadQuickLocateRqsts(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // A.Kinney	04/29/97	Changed Current_Date
    // C. Ott          02/23/99        Modified for 'Update' function
    // M.Ashworth      11/19/01        Modified Read in order to fix -811 
    // PR128345
    // ****************************************************
    local.Current.Date = Now().Date;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    UseOeCabSetMnemonics();

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------
    // If the Case contains multiple AP's, flow to
    // the Case_Composition screen.
    // ---------------------------------------------
    if (IsEmpty(import.Ap.Number))
    {
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        foreach(var item in ReadCsePersonAbsentParent4())
        {
          export.Ap.Number = entities.Ap.Number;
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
        }
      }
      else
      {
        foreach(var item in ReadCsePersonAbsentParent3())
        {
          export.Ap.Number = entities.Ap.Number;
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
        }
      }
    }

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      // ****************************************************
      // M.Ashworth      11/19/01        Modified Read in order to fix -811 
      // PR128345
      // ****************************************************
      if (ReadCsePersonAbsentParent1())
      {
        goto Test1;
      }

      // ****************************************************
      // It will only reach this code if the ap is not found
      // ****************************************************
      ExitState = "AP_FOR_CASE_NF";

      return;
    }
    else if (!ReadCsePersonAbsentParent2())
    {
      ExitState = "AP_FOR_CASE_NF";

      return;
    }

Test1:

    export.Ap.Number = entities.Ap.Number;
    UseSiReadCsePerson1();

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      // ****************************************************
      // M.Ashworth      11/19/01        Modified Read in order to fix -811 
      // PR128345
      // ****************************************************
      if (ReadCsePersonApplicantRecipient1())
      {
        goto Test2;
      }

      // ****************************************************
      // It will only reach this code if the ar is not found
      // ****************************************************
      ExitState = "AR_DB_ERROR_NF";

      return;
    }
    else if (!ReadCsePersonApplicantRecipient2())
    {
      ExitState = "AR_DB_ERROR_NF";

      return;
    }

Test2:

    export.Ar.Number = entities.Ar.Number;
    UseSiReadCsePerson2();

    // ***************************************************************
    // The following READ property is both select and cursor because it is 
    // simply a check for the presence of any Intersate Requests.
    // ****************************************************************
    if (ReadInterstateRequest())
    {
      // ***************************************************************
      // 02/03/1999   C. Ott
      // Removed check for incoming Interstate Case.  Business rules state that 
      // Quick Locate Requests may be sent for either incoming or outgoing
      // Interstate Cases.
      // ***************************************************************
    }
    else
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

      return;
    }

    export.Export1.Index = -1;

    foreach(var item in ReadInterstateRequestHistoryInterstateRequest())
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (ReadFips())
      {
        export.Export1.Update.State.State = entities.Fips.StateAbbreviation;
      }

      export.Export1.Update.Hidden.IntHGeneratedId =
        entities.InterstateRequest.IntHGeneratedId;
      MoveInterstateRequestHistory(entities.Send, export.Export1.Update.Send);
      local.MultipleResponse.Flag = "";

      if (entities.Send.ActionResolutionDate != null)
      {
        export.Export1.Update.Return1.TransactionDate =
          entities.Send.ActionResolutionDate;

        continue;
      }

      if (ReadInterstateRequestHistory())
      {
        export.Export1.Update.Return1.TransactionDate =
          entities.Return1.TransactionDate;
      }
    }

    if (!export.Export1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.CheckSize();

      if (!IsEmpty(export.Export1.Item.State.State))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        return;
      }
    }

    ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.LastLo1PDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
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
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePersonAbsentParent1()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadCsePersonAbsentParent1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadCsePersonAbsentParent2()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadCsePersonAbsentParent2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent3()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent4()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCsePersonApplicantRecipient1()
  {
    entities.Ar.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCsePersonApplicantRecipient1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCsePersonApplicantRecipient2()
  {
    entities.Ar.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCsePersonApplicantRecipient2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetInt32(command, "state", entities.InterstateRequest.OtherStateFips);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.Return1.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "transactionDate",
          entities.Send.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Return1.IntGeneratedId = db.GetInt32(reader, 0);
        entities.Return1.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Return1.ActionCode = db.GetString(reader, 2);
        entities.Return1.FunctionalTypeCode = db.GetString(reader, 3);
        entities.Return1.TransactionDate = db.GetDate(reader, 4);
        entities.Return1.ActionResolutionDate = db.GetNullableDate(reader, 5);
        entities.Return1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistoryInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;
    entities.Send.Populated = false;

    return ReadEach("ReadInterstateRequestHistoryInterstateRequest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.Send.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.Send.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Send.ActionCode = db.GetString(reader, 2);
        entities.Send.FunctionalTypeCode = db.GetString(reader, 3);
        entities.Send.TransactionDate = db.GetDate(reader, 4);
        entities.Send.ActionResolutionDate = db.GetNullableDate(reader, 5);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 8);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 9);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 11);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 12);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 13);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 16);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 17);
        entities.InterstateRequest.Populated = true;
        entities.Send.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Common caseOpen;
    private Case1 case1;
    private CsePersonsWorkSet ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Common State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of Send.
      /// </summary>
      [JsonPropertyName("send")]
      public InterstateRequestHistory Send
      {
        get => send ??= new();
        set => send = value;
      }

      /// <summary>
      /// A value of Return1.
      /// </summary>
      [JsonPropertyName("return1")]
      public InterstateRequestHistory Return1
      {
        get => return1 ??= new();
        set => return1 = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public InterstateRequest Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common state;
      private InterstateRequestHistory send;
      private InterstateRequestHistory return1;
      private InterstateRequest hidden;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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

    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Array<ExportGroup> export1;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of MultipleResponse.
    /// </summary>
    [JsonPropertyName("multipleResponse")]
    public Common MultipleResponse
    {
      get => multipleResponse ??= new();
      set => multipleResponse = value;
    }

    /// <summary>
    /// A value of LastLo1PDate.
    /// </summary>
    [JsonPropertyName("lastLo1PDate")]
    public Code LastLo1PDate
    {
      get => lastLo1PDate ??= new();
      set => lastLo1PDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private DateWorkArea current;
    private Common multipleResponse;
    private Code lastLo1PDate;
    private Common common;
    private Fips fips;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public InterstateRequestHistory Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public InterstateRequestHistory Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    private Fips fips;
    private Case1 case1;
    private CsePerson ar;
    private CsePerson ap;
    private CaseRole absentParent;
    private InterstateRequest interstateRequest;
    private CaseRole applicantRecipient;
    private InterstateRequestHistory send;
    private InterstateRequestHistory return1;
  }
#endregion
}
