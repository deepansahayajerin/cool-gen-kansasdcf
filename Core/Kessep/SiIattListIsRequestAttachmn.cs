// Program: SI_IATT_LIST_IS_REQUEST_ATTACHMN, ID: 372381423, model: 746.
// Short name: SWE01186
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
/// A program: SI_IATT_LIST_IS_REQUEST_ATTACHMN.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiIattListIsRequestAttachmn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_LIST_IS_REQUEST_ATTACHMN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattListIsRequestAttachmn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattListIsRequestAttachmn.
  /// </summary>
  public SiIattListIsRequestAttachmn(IContext context, Import import,
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
    // ------------------------------------------------------------
    // 04/29/97    JeHoward          Current date fix.
    // ------------------------------------------------------------
    local.NonLo1.Flag = "N";
    local.Current.Date = Now().Date;
    export.Ap.Number = import.Ap.Number;
    export.OtherState.StateAbbreviation = import.OtherState.StateAbbreviation;
    MoveInterstateRequest(import.InterstateRequest, export.InterstateRequest);
    export.State.State = export.OtherState.StateAbbreviation;

    if (!IsEmpty(export.OtherState.StateAbbreviation))
    {
      if (ReadFips2())
      {
        MoveFips(entities.Fips, local.Fips);
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }
    }

    if (ReadCase())
    {
      export.Case1.Number = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ***   Check for multiple AP in the current Case   ***
    if (IsEmpty(import.Ap.Number))
    {
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        foreach(var item in ReadAbsentParentCsePerson4())
        {
          if (AsChar(local.MultipleAp.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            export.Ap.Number = entities.Ap.Number;
            local.MultipleAp.Flag = "Y";
          }
        }
      }
      else
      {
        foreach(var item in ReadAbsentParentCsePerson3())
        {
          if (AsChar(local.MultipleAp.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            export.Ap.Number = entities.Ap.Number;
            local.MultipleAp.Flag = "Y";
          }
        }
      }
    }

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      if (!ReadAbsentParentCsePerson1())
      {
        ExitState = "AP_FOR_CASE_NF";

        return;
      }
    }
    else if (!ReadAbsentParentCsePerson2())
    {
      ExitState = "AP_FOR_CASE_NF";

      return;
    }

    export.Ap.Number = entities.Ap.Number;
    UseSiReadCsePerson2();

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      if (!ReadApplicantRecipientCsePerson1())
      {
        ExitState = "AR_DB_ERROR_NF";

        return;
      }
    }
    else if (!ReadApplicantRecipientCsePerson2())
    {
      ExitState = "AR_DB_ERROR_NF";

      return;
    }

    export.Ar.Number = entities.Ar.Number;
    UseSiReadCsePerson1();

    if (IsEmpty(export.OtherState.StateAbbreviation))
    {
      local.NonLo1.Flag = "N";
      local.IsLo1.Flag = "N";

      foreach(var item in ReadInterstateCase3())
      {
        local.NonLo1.Flag = "Y";
      }

      foreach(var item in ReadInterstateCase2())
      {
        local.IsLo1.Flag = "Y";
      }

      local.MultipleIntRequest.Flag = "N";

      foreach(var item in ReadInterstateRequest2())
      {
        if (AsChar(local.MultipleIntRequest.Flag) == 'Y')
        {
          export.InterstateRequest.Assign(local.Refresh);
          export.State.State = "";
          ExitState = "SI0000_MULTIPLE_IR_EXISTS_FOR_AP";

          return;
        }

        export.InterstateRequest.Assign(entities.InterstateRequest);

        if (export.InterstateRequest.OtherStateFips > 0)
        {
          if (ReadFips1())
          {
            MoveFips(entities.Fips, local.Fips);
            export.State.State = entities.Fips.StateAbbreviation;
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }
        }

        local.MultipleIntRequest.Flag = "Y";

        if (local.Fips.State == entities.InterstateRequest.OtherStateFips && local
          .Fips.State != 0)
        {
          export.State.State = entities.Fips.StateAbbreviation;
          export.InterstateRequest.Assign(entities.InterstateRequest);
        }
      }

      if (AsChar(local.MultipleIntRequest.Flag) == 'N')
      {
        ExitState = "CASE_NOT_INTERSTATE";

        return;
      }
      else if (AsChar(local.IsLo1.Flag) == 'Y' && AsChar(local.NonLo1.Flag) == 'N'
        )
      {
        ExitState = "CASE_NOT_INTERSTATE";

        return;
      }
    }
    else
    {
      if (ReadFips2())
      {
        MoveFips(entities.Fips, local.Fips);
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }

      local.NonLo1.Flag = "N";

      foreach(var item in ReadInterstateCase1())
      {
        local.NonLo1.Flag = "Y";
      }

      if (ReadInterstateRequest1())
      {
        export.InterstateRequest.Assign(entities.InterstateRequest);

        if (export.InterstateRequest.OtherStateFips > 0)
        {
          if (ReadFips1())
          {
            export.State.State = entities.Fips.StateAbbreviation;
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }
        }

        if (local.Fips.State == entities.InterstateRequest.OtherStateFips && local
          .Fips.State != 0)
        {
          export.State.State = entities.Fips.StateAbbreviation;
          export.InterstateRequest.Assign(entities.InterstateRequest);
        }
      }
      else
      {
        ExitState = "SI0000_NO_IC_WITH_OTHER_STATE";

        return;
      }
    }

    if (export.InterstateRequest.OtherStateFips > 0)
    {
      if (ReadFips1())
      {
        MoveFips(entities.Fips, local.Fips);
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }

      if (local.Fips.State == entities.InterstateRequest.OtherStateFips && local
        .Fips.State != 0)
      {
        export.State.State = entities.Fips.StateAbbreviation;
        export.InterstateRequest.Assign(entities.InterstateRequest);
      }
    }
    else
    {
      export.State.State = import.OtherState.StateAbbreviation;
      ExitState = "ACO_NI0000_NO_RECORDS_FOUND";

      return;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadInterstateRequestAttachment())
    {
      export.Export1.Update.Details.
        Assign(entities.InterstateRequestAttachment);
      export.Export1.Next();
    }

    if (!export.Export1.IsEmpty)
    {
      if (AsChar(export.InterstateRequest.KsCaseInd) == 'Y')
      {
        ExitState = "SI0000_DISPLAY_OK_FOR_OG_INT_CAS";
      }
      else
      {
        ExitState = "SI0000_DISPLAY_OK_FOR_IC_INT_CAS";
      }
    }
    else
    {
      export.State.State = import.OtherState.StateAbbreviation;
      ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private bool ReadAbsentParentCsePerson1()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadAbsentParentCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadAbsentParentCsePerson2()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadAbsentParentCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadAbsentParentCsePerson3()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
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

  private IEnumerable<bool> ReadAbsentParentCsePerson4()
  {
    entities.Ap.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
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

  private bool ReadApplicantRecipientCsePerson1()
  {
    entities.Ar.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipientCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson2()
  {
    entities.Ar.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipientCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
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

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.InterstateRequest.OtherStateFips);
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

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.OtherState.StateAbbreviation);
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

  private IEnumerable<bool> ReadInterstateCase1()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase1",
      (db, command) =>
      {
        db.SetString(command, "number", entities.Case1.Number);
        db.SetInt32(command, "otherFipsState", local.Fips.State);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 2);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 3);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase2()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase2",
      (db, command) =>
      {
        db.SetString(command, "number", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 2);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 3);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase3()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase3",
      (db, command) =>
      {
        db.SetString(command, "number", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 2);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 3);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(command, "othrStateFipsCd", local.Fips.State);
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

  private IEnumerable<bool> ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest2",
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

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAttachment()
  {
    return ReadEach("ReadInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId", export.InterstateRequest.IntHGeneratedId);
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.SentDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateRequestAttachment.RequestDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequestAttachment.ReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.InterstateRequestAttachment.DataTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestAttachment.Note =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestAttachment.Populated = true;

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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

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

    private InterstateRequest interstateRequest;
    private Fips otherState;
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
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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

    private Fips otherState;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private Common state;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IsLo1.
    /// </summary>
    [JsonPropertyName("isLo1")]
    public Common IsLo1
    {
      get => isLo1 ??= new();
      set => isLo1 = value;
    }

    /// <summary>
    /// A value of NonLo1.
    /// </summary>
    [JsonPropertyName("nonLo1")]
    public Common NonLo1
    {
      get => nonLo1 ??= new();
      set => nonLo1 = value;
    }

    /// <summary>
    /// A value of Refresh.
    /// </summary>
    [JsonPropertyName("refresh")]
    public InterstateRequest Refresh
    {
      get => refresh ??= new();
      set => refresh = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of MultipleAp.
    /// </summary>
    [JsonPropertyName("multipleAp")]
    public Common MultipleAp
    {
      get => multipleAp ??= new();
      set => multipleAp = value;
    }

    /// <summary>
    /// A value of MultipleIntRequest.
    /// </summary>
    [JsonPropertyName("multipleIntRequest")]
    public Common MultipleIntRequest
    {
      get => multipleIntRequest ??= new();
      set => multipleIntRequest = value;
    }

    private Common isLo1;
    private Common nonLo1;
    private InterstateRequest refresh;
    private Fips fips;
    private DateWorkArea current;
    private Common multipleAp;
    private Common multipleIntRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of InterstateRequestAttachment.
    /// </summary>
    [JsonPropertyName("interstateRequestAttachment")]
    public InterstateRequestAttachment InterstateRequestAttachment
    {
      get => interstateRequestAttachment ??= new();
      set => interstateRequestAttachment = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private Fips fips;
    private Case1 case1;
    private CsePerson ar;
    private CsePerson ap;
    private CaseRole absentParent;
    private InterstateRequest interstateRequest;
    private CaseRole applicantRecipient;
    private InterstateRequestAttachment interstateRequestAttachment;
  }
#endregion
}
