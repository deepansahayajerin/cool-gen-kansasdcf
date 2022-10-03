// Program: FN_LTRN_LIST_EFT_TRANSMISSIONS, ID: 372413419, model: 746.
// Short name: SWELTRNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_LTRN_LIST_EFT_TRANSMISSIONS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLtrnListEftTransmissions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LTRN_LIST_EFT_TRANSMISSIONS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLtrnListEftTransmissions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLtrnListEftTransmissions.
  /// </summary>
  public FnLtrnListEftTransmissions(IContext context, Import import,
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
    // ***************************************************************************
    // DATE       NAME			DESCRIPTION
    // 06/10/97   A Samuels (MTW)	Initial Development	
    // 12/14/98   Fangman              Rewritten
    // ***************************************************************************
    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        var field =
          GetField(export.SearchElectronicFundTransmission, "transmissionType");
          

        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        break;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // *****
    // Move imports to exports
    // *****
    MoveStandard(import.Standard, export.Standard);
    export.SearchElectronicFundTransmission.Assign(import.Search);

    if (Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.SearchElectronicFundTransmission.TransmissionType) == 'O'
        )
      {
        ExitState = "ECO_XFR_TO_DISB_MGMNT_MENU";
      }
      else
      {
        ExitState = "ECO_XFR_TO_CASH_MGMNT_MENU";
      }

      return;
    }
    else
    {
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      export.SearchElectronicFundTransmission.TransmissionStatusCode =
        import.FromList.Cdvalue;
      export.Standard.PromptField = "";
      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      local.Common.Count = 0;

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.ElectronicFundTransmission.Assign(
          import.Group.Item.ElectronicFundTransmission);
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        // *****
        // Determine if a selection has been made
        // *****
        if (!IsEmpty(import.Group.Item.Common.SelectChar))
        {
          ++local.Common.Count;

          if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
          {
            MoveElectronicFundTransmission(import.Group.Item.
              ElectronicFundTransmission, export.Selection);
            export.Selection.TransmissionType =
              export.SearchElectronicFundTransmission.TransmissionType;
          }
          else
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        export.Group.Next();
      }
    }

    if (local.Common.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****
    // Next Tran/Security logic
    // *****
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      var field =
        GetField(export.SearchElectronicFundTransmission, "transmissionType");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **************************************
    // Validate search criteria.
    // **************************************
    if (Lt(local.Current.Date,
      export.SearchElectronicFundTransmission.FileCreationDate))
    {
      var field =
        GetField(export.SearchElectronicFundTransmission, "fileCreationDate");

      field.Error = true;

      ExitState = "FN0000_DATE_MUST_BE_LT_CURRENT";
    }

    if (!IsEmpty(import.Search.TransmissionStatusCode))
    {
      // **************************************
      // Validate status code entered.
      // **************************************
      local.ValidateCode.CodeName = "EFT TRANSMISSION STATUS";
      local.ValidateCodeValue.Cdvalue = import.Search.TransmissionStatusCode;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        var field =
          GetField(export.SearchElectronicFundTransmission,
          "transmissionStatusCode");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
      }
    }

    switch(AsChar(import.Search.TransmissionType))
    {
      case 'I':
        break;
      case 'O':
        break;
      case ' ':
        export.SearchElectronicFundTransmission.TransmissionType = "I";

        break;
      default:
        var field =
          GetField(export.SearchElectronicFundTransmission, "transmissionType");
          

        field.Error = true;

        ExitState = "SP0000_INVALID_TYPE_CODE";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (export.SearchElectronicFundTransmission.FileCreationTime.
          GetValueOrDefault() > local.Null1.Time)
        {
          if (Lt(local.Null1.Date,
            export.SearchElectronicFundTransmission.FileCreationDate))
          {
            switch(TrimEnd(export.SearchElectronicFundTransmission.
              TransmissionStatusCode))
            {
              case "":
                // Read all, Sort Descending by date
                export.Group.Index = 0;
                export.Group.Clear();

                foreach(var item in ReadElectronicFundTransmission5())
                {
                  export.Group.Update.ElectronicFundTransmission.Assign(
                    entities.ElectronicFundTransmission);
                  export.Group.Next();
                }

                break;
              case "PENDED":
                // Read Pended, Sort Ascending by date
                export.Group.Index = 0;
                export.Group.Clear();

                foreach(var item in ReadElectronicFundTransmission1())
                {
                  export.Group.Update.ElectronicFundTransmission.Assign(
                    entities.ElectronicFundTransmission);
                  export.Group.Next();
                }

                break;
              default:
                // Read by status code, Sort Descending by date
                export.Group.Index = 0;
                export.Group.Clear();

                foreach(var item in ReadElectronicFundTransmission4())
                {
                  export.Group.Update.ElectronicFundTransmission.Assign(
                    entities.ElectronicFundTransmission);
                  export.Group.Next();
                }

                break;
            }
          }
          else
          {
            // Error - Cannot have a search time without a search date.
          }
        }
        else if (Lt(local.Null1.Date,
          export.SearchElectronicFundTransmission.FileCreationDate))
        {
          switch(TrimEnd(export.SearchElectronicFundTransmission.
            TransmissionStatusCode))
          {
            case "":
              // Read all, Sort Descending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission7())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
            case "PENDED":
              // Read Pended, Sort Ascending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission2())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
            default:
              // Read by status code, Sort Descending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission6())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
          }
        }
        else
        {
          switch(TrimEnd(export.SearchElectronicFundTransmission.
            TransmissionStatusCode))
          {
            case "":
              // Read all, Sort Descending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission9())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
            case "PENDED":
              // Read Pended, Sort Ascending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission3())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
            default:
              // Read by status code, Sort Descending by date
              export.Group.Index = 0;
              export.Group.Clear();

              foreach(var item in ReadElectronicFundTransmission8())
              {
                export.Group.Update.ElectronicFundTransmission.Assign(
                  entities.ElectronicFundTransmission);
                export.Group.Next();
              }

              break;
          }
        }

        // *****
        // If group view is empty display message.  If group view is full 
        // display message.
        // *****
        if (export.Group.IsEmpty)
        {
          ExitState = "FN0000_NO_EFT_RECORD";
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "MTRN":
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else
        {
          ExitState = "ECO_LNK_MTN_EFT_TRANSMISSION";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "LIST":
        if (!IsEmpty(import.Standard.PromptField))
        {
          if (AsChar(import.Standard.PromptField) == 'S')
          {
            export.ToList.CodeName = "EFT TRANSMISSION STATUS";
            ExitState = "ECO_LNK_TO_CDVL";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "RETURN":
        if (!IsEmpty(export.Selection.TransmissionType))
        {
          export.SearchEftTransmissionFileInfo.TransmissionType =
            export.Selection.TransmissionType;
          export.SearchEftTransmissionFileInfo.FileCreationDate =
            export.Selection.FileCreationDate;
          export.SearchEnding.FileCreationDate =
            export.Selection.FileCreationDate;
        }
        else
        {
          export.SearchEftTransmissionFileInfo.TransmissionType =
            export.SearchElectronicFundTransmission.TransmissionType;
          export.SearchEftTransmissionFileInfo.FileCreationDate =
            Now().Date.AddMonths(-1);
          export.SearchEnding.FileCreationDate = Now().Date;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LEFT":
        if (!IsEmpty(export.Selection.TransmissionType))
        {
          export.SearchEftTransmissionFileInfo.TransmissionType =
            export.Selection.TransmissionType;
          export.SearchEftTransmissionFileInfo.FileCreationDate =
            export.Selection.FileCreationDate;
          export.SearchEnding.FileCreationDate =
            export.Selection.FileCreationDate;
        }
        else
        {
          export.SearchEftTransmissionFileInfo.TransmissionType =
            export.SearchElectronicFundTransmission.TransmissionType;
          export.SearchEftTransmissionFileInfo.FileCreationDate =
            export.SearchElectronicFundTransmission.FileCreationDate;
          export.SearchEnding.FileCreationDate =
            export.SearchElectronicFundTransmission.FileCreationDate;
        }

        ExitState = "ECO_LNK_LST_EFT_TRAN_INFO";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.FileCreationDate = source.FileCreationDate;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadElectronicFundTransmission1()
  {
    return ReadEach("ReadElectronicFundTransmission1",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetNullableTimeSpan(
          command, "fileCreationTime",
          export.SearchElectronicFundTransmission.FileCreationTime.
            GetValueOrDefault());
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission2()
  {
    return ReadEach("ReadElectronicFundTransmission2",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission3()
  {
    return ReadEach("ReadElectronicFundTransmission3",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission4()
  {
    return ReadEach("ReadElectronicFundTransmission4",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetNullableTimeSpan(
          command, "fileCreationTime",
          export.SearchElectronicFundTransmission.FileCreationTime.
            GetValueOrDefault());
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission5()
  {
    return ReadEach("ReadElectronicFundTransmission5",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetNullableTimeSpan(
          command, "fileCreationTime",
          export.SearchElectronicFundTransmission.FileCreationTime.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission6()
  {
    return ReadEach("ReadElectronicFundTransmission6",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission7()
  {
    return ReadEach("ReadElectronicFundTransmission7",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableDate(
          command, "fileCreationDate",
          export.SearchElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission8()
  {
    return ReadEach("ReadElectronicFundTransmission8",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetString(
          command, "transStatusCode",
          export.SearchElectronicFundTransmission.TransmissionStatusCode);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission9()
  {
    return ReadEach("ReadElectronicFundTransmission9",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.SearchElectronicFundTransmission.TransmissionType);
        db.SetNullableInt32(
          command, "sequenceNumber",
          export.SearchElectronicFundTransmission.SequenceNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 1);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 3);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ElectronicFundTransmission.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ElectronicFundTransmission.
      /// </summary>
      [JsonPropertyName("electronicFundTransmission")]
      public ElectronicFundTransmission ElectronicFundTransmission
      {
        get => electronicFundTransmission ??= new();
        set => electronicFundTransmission = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private ElectronicFundTransmission electronicFundTransmission;
    }

    /// <summary>
    /// A value of FromList.
    /// </summary>
    [JsonPropertyName("fromList")]
    public CodeValue FromList
    {
      get => fromList ??= new();
      set => fromList = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ElectronicFundTransmission Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private CodeValue fromList;
    private Standard standard;
    private ElectronicFundTransmission search;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ElectronicFundTransmission.
      /// </summary>
      [JsonPropertyName("electronicFundTransmission")]
      public ElectronicFundTransmission ElectronicFundTransmission
      {
        get => electronicFundTransmission ??= new();
        set => electronicFundTransmission = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private ElectronicFundTransmission electronicFundTransmission;
    }

    /// <summary>
    /// A value of ToList.
    /// </summary>
    [JsonPropertyName("toList")]
    public Code ToList
    {
      get => toList ??= new();
      set => toList = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of SearchElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("searchElectronicFundTransmission")]
    public ElectronicFundTransmission SearchElectronicFundTransmission
    {
      get => searchElectronicFundTransmission ??= new();
      set => searchElectronicFundTransmission = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public ElectronicFundTransmission Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    /// <summary>
    /// A value of SearchEftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("searchEftTransmissionFileInfo")]
    public EftTransmissionFileInfo SearchEftTransmissionFileInfo
    {
      get => searchEftTransmissionFileInfo ??= new();
      set => searchEftTransmissionFileInfo = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public EftTransmissionFileInfo SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Code toList;
    private Standard standard;
    private ElectronicFundTransmission searchElectronicFundTransmission;
    private Array<GroupGroup> group;
    private ElectronicFundTransmission selection;
    private EftTransmissionFileInfo searchEftTransmissionFileInfo;
    private EftTransmissionFileInfo searchEnding;
    private NextTranInfo hidden;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ElectronicFundTransmission Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private ElectronicFundTransmission initialized;
    private Common validCode;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private DateWorkArea null1;
    private DateWorkArea current;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
  }
#endregion
}
