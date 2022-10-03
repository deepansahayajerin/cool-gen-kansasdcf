// Program: SP_PRINT_DATA_RETRIEVAL_CASE, ID: 372132890, model: 746.
// Short name: SWE02267
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_CASE.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalCase.
  /// </summary>
  public SpPrintDataRetrievalCase(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------
    // 12/02/1998	M Ramirez			Initial Development
    // 07/14/1999	M Ramirez			Added row lock counts
    // 10/01/1999	M Ramirez	74734		Reset exitstate received
    // 						from si_read_case_program_type
    // 10/13/1999	M Ramirez	772255		Added case closure reason field
    // 01/12/2000	P McElderry	77873		Allowing 4E cases to print IV-D Taf
    // 09/13/2000	M Ramirez	103095		Added field CASE/_DETERMN/CHILDCASC
    // 2/18/2002	K Cole		PR138671	Populate case closure reason even if case is 
    // open
    // 11/17/2011	G Vandy		CQ30161		Add support for CASETXFR document fields
    // ------------------------------------------------------------------------------
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.SpDocKey.Assign(import.SpDocKey);

    foreach(var item in ReadField())
    {
      // mjr--->  For Fields processed in this CAB
      if (Lt(entities.Field.SubroutineName, local.PreviousField.SubroutineName) ||
        Equal
        (entities.Field.SubroutineName, local.PreviousField.SubroutineName) && !
        Lt(local.PreviousField.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";
      MoveField(entities.Field, local.PreviousField);

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case " DETERMN":
          if (!IsEmpty(import.SpDocKey.KeyCase))
          {
            local.FieldValue.Value = import.SpDocKey.KeyCase;
          }
          else
          {
            switch(TrimEnd(entities.Field.Name))
            {
              case "AAPPCSECAS":
                // mjr----> Find case from Admin Appeal
                local.Count.Count = 0;

                foreach(var item1 in ReadCase12())
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 1)
                  {
                    local.Case1.Number = "";

                    break;
                  }

                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "APTCASENUM":
                // mjr----> Find case from Appointment
                if (ReadCase7())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "CHILDCASC":
                // mjr----> Find case with Client as AR from Child Number
                foreach(var item1 in ReadCase11())
                {
                  if (ReadCaseRoleCsePerson())
                  {
                    local.Case1.Assign(entities.Case1);

                    break;
                  }
                }

                break;
              case "CHILDCASE":
                // mjr----> Find case from Child Number
                if (ReadCase2())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "GTCSECASE":
                // mjr----> Find case from Genetic Test
                if (ReadCase6())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "INTSTRQCAS":
                // mjr----> Find case from Interstate Request
                if (ReadCase8())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "LEGALACTCA":
                // mjr----> Find case from Legal Action
                if (ReadCase1())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "LEGALNOACA":
                // mjr----> Find case from Legal Action
                if (ReadCase3())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "PERSONCASE":
                // mjr----> Find case from a person number
                if (!IsEmpty(export.SpDocKey.KeyAp))
                {
                  local.CsePerson.Number = export.SpDocKey.KeyAp;
                  local.CaseRole.Type1 = "AP";
                }
                else if (!IsEmpty(export.SpDocKey.KeyAr))
                {
                  local.CsePerson.Number = export.SpDocKey.KeyAr;
                  local.CaseRole.Type1 = "AR";
                }
                else if (!IsEmpty(export.SpDocKey.KeyChild))
                {
                  local.CsePerson.Number = export.SpDocKey.KeyChild;
                  local.CaseRole.Type1 = "CH";
                }
                else if (!IsEmpty(export.SpDocKey.KeyPerson))
                {
                  local.CsePerson.Number = export.SpDocKey.KeyPerson;

                  if (ReadCase5())
                  {
                    local.Case1.Assign(entities.Case1);
                  }

                  break;
                }
                else
                {
                  break;
                }

                if (ReadCase4())
                {
                  local.Case1.Assign(entities.Case1);
                }

                break;
              case "CASXFRAP0":
                // 11/17/2011  G Vandy  CQ30161  Add support for CASETXFR 
                // document fields.
                // -- Find all case and AP numbers for the KEY_AR which were
                //    transferred to KEY_OFFICE between KEY_XFER_FROM_DATE to 
                // KEY_XFER_TO_DATE.
                local.CaseTransfer.Index = -1;
                local.ProcessGroup.Flag = "X";

                foreach(var item1 in ReadCase10())
                {
                  // -- Skip the case if it is assigned to the same office as on
                  // the last run date.
                  if (ReadOffice())
                  {
                    if (entities.Old.SystemGeneratedId == import
                      .SpDocKey.KeyOffice)
                    {
                      continue;
                    }
                  }
                  else
                  {
                    // If there was no caseworker assignment on the last run 
                    // date then skip the case.
                    continue;
                  }

                  // -- Exclude incoming interstate cases.
                  foreach(var item2 in ReadInterstateRequest())
                  {
                    goto ReadEach1;
                  }

                  if (local.CaseTransfer.Index + 1 >= Local
                    .CaseTransferGroup.Capacity)
                  {
                    break;
                  }
                  else
                  {
                    ++local.CaseTransfer.Index;
                    local.CaseTransfer.CheckSize();
                  }

                  local.CaseTransfer.Update.GlocalCase.Value =
                    entities.Case1.Number;

                  if (IsEmpty(local.Case1.Number))
                  {
                    local.Case1.Number = entities.Case1.Number;
                  }

                  local.Common.Count = 1;

                  foreach(var item2 in ReadCsePerson())
                  {
                    if (local.Common.Count == 1)
                    {
                    }
                    else if (local.CaseTransfer.Index + 1 >= Local
                      .CaseTransferGroup.Capacity)
                    {
                      goto ReadEach2;
                    }
                    else
                    {
                      ++local.CaseTransfer.Index;
                      local.CaseTransfer.CheckSize();
                    }

                    ++local.Common.Count;

                    // -- Call SI_READ_CSE_PERSON to get AP name from adabas.
                    local.CsePersonsWorkSet.Number =
                      entities.ApCsePerson.Number;
                    UseSiReadCsePerson();
                    local.CaseTransfer.Update.GlocalApName.Value =
                      local.CsePersonsWorkSet.FormattedName;

                    if (!IsEmpty(local.AbendData.Type1) || !
                      IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "ACO_NN0000_ALL_OK";
                    }

                    if (!IsEmpty(local.AbendData.Type1) && Equal
                      (local.AbendData.AdabasResponseCd, "0148"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Resource Error";
                      export.ErrorFieldValue.Value = "ADABAS Unavailable";

                      return;
                    }
                  }

ReadEach1:
                  ;
                }

ReadEach2:

                break;
              default:
                export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
                export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                  (entities.Field.Name) + ",  Subroutine:  " + entities
                  .Field.SubroutineName;

                break;
            }

            if (IsEmpty(local.Case1.Number))
            {
              return;
            }

            export.SpDocKey.KeyCase = local.Case1.Number;
            local.FieldValue.Value = local.Case1.Number;
          }

          break;
        case "CASE":
          if (IsEmpty(local.Case1.Number))
          {
            // mjr
            // ---------------------------------------------------
            // Extract case number from next_tran_info
            // ------------------------------------------------------
            if (ReadCase9())
            {
              local.Case1.Assign(entities.Case1);
            }
            else
            {
              // mjr---> Case not found, but no message is given.
              return;
            }
          }

          if (Equal(entities.Field.Name, 1, 4, "CAPR"))
          {
            local.Field.Name = "CAPR*" + Substring
              (entities.Field.Name, Field.Name_MaxLength, 8, 3);
            local.Program.Code = Substring(entities.Field.Name, 5, 3);

            if (!Equal(local.Program.Code, entities.Program.Code))
            {
              // mjr
              // ----------------------------------------------------
              // 10/18/1999
              // Find the oldest <program_code> program related to the case
              // -----------------------------------------------------------------
              local.PersonProgram.DiscontinueDate = local.Null1.Date;
              local.PersonProgram.EffectiveDate = local.Null1.Date;

              if (ReadPersonProgramProgram())
              {
                local.PersonProgram.Assign(entities.PersonProgram);
                local.Program.Code = entities.Program.Code;
              }
            }
          }
          else
          {
            local.Field.Name = entities.Field.Name;
            local.Program.Code = "";
          }

          switch(TrimEnd(local.Field.Name))
          {
            case "CAADCOPNDT":
              if (Lt(local.Null1.Date, local.Case1.CseOpenDate))
              {
                local.DateWorkArea.Date = local.Case1.CseOpenDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CAADCPART":
              if (!IsEmpty(local.Case1.Number))
              {
                UseSiReadCaseProgramType();

                if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                  local.FieldValue.Value = "Y";
                }
                else if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "MAI") || Equal
                  (local.Program.Code, "NAI") || Equal
                  (local.Program.Code, "NF"))
                {
                  local.FieldValue.Value = "N";
                }
                else
                {
                  local.FieldValue.Value = "Y";
                }
              }

              break;
            case "CACLOSECOD":
              if (!Equal(local.Case1.ClosureReason, "CV"))
              {
                local.FieldValue.Value = local.Case1.ClosureReason ?? "";
              }

              break;
            case "CAPR*FDT":
              if (Lt(local.Null1.Date, local.PersonProgram.EffectiveDate))
              {
                local.DateWorkArea.Date = local.PersonProgram.EffectiveDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CAPR*IND":
              if (Lt(local.Null1.Date, local.PersonProgram.EffectiveDate))
              {
                local.FieldValue.Value = "Y";
              }
              else
              {
                local.FieldValue.Value = "N";
              }

              break;
            case "CAPR*XDT":
              if (Lt(local.Null1.Date, local.PersonProgram.DiscontinueDate))
              {
                local.DateWorkArea.Date = local.PersonProgram.DiscontinueDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CASTATDT":
              if (Lt(local.Null1.Date, local.Case1.StatusDate))
              {
                local.DateWorkArea.Date = local.Case1.StatusDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CASTDT+1":
              if (Lt(local.Null1.Date, local.Case1.StatusDate))
              {
                local.DateWorkArea.Date = AddDays(local.Case1.StatusDate, 1);
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CSECASENUM":
              local.FieldValue.Value = local.Case1.Number;

              break;
            case "KAECSESNUM":
              if (ReadOldNewXref())
              {
                local.FieldValue.Value = entities.OldNewXref.CaecsesCaseNbr;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;
          ExitState = "FIELD_NF";

          break;
      }

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(AsChar(local.ProcessGroup.Flag))
      {
        case 'A':
          // mjr
          // ----------------------------------------------
          // Field is an address
          //    Process 1-5 of group_local_address
          // -------------------------------------------------
          local.Position.Count = Length(TrimEnd(entities.Field.Name));
          local.CurrentRoot.Name =
            Substring(entities.Field.Name, 1, local.Position.Count - 1);

          for(local.Address.Index = 0; local.Address.Index < local
            .Address.Count; ++local.Address.Index)
          {
            if (!local.Address.CheckSize())
            {
              break;
            }

            // mjr---> Increment Field Name
            local.Temp.Name = NumberToString(local.Address.Index + 1, 10);
            local.Position.Count = Verify(local.Temp.Name, "0");
            local.Temp.Name =
              Substring(local.Temp.Name, local.Position.Count, 16 -
              local.Position.Count);
            local.Current.Name = TrimEnd(local.CurrentRoot.Name) + local
              .Temp.Name;
            UseSpCabCreateUpdateFieldValue3();

            if (IsExitState("DOCUMENT_FIELD_NF_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.ErrorDocumentField.ScreenPrompt = "Creation Error";
              export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

              return;
            }

            local.Address.Update.GlocalAddress.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Address.CheckIndex();

          break;
        case 'X':
          // -------------------------------------------------
          // Field is a case transfer list of cases and AP names.
          // -------------------------------------------------
          local.CaseTransfer.Index = 0;

          for(var limit = local.CaseTransfer.Count; local.CaseTransfer.Index < limit
            ; ++local.CaseTransfer.Index)
          {
            if (!local.CaseTransfer.CheckSize())
            {
              break;
            }

            for(local.Common.Count = 1; local.Common.Count <= 2; ++
              local.Common.Count)
            {
              switch(local.Common.Count)
              {
                case 1:
                  local.Current.Name = "CASXFRCAS" + NumberToString
                    (local.CaseTransfer.Index, 15, 1);
                  local.FieldValue.Value =
                    local.CaseTransfer.Item.GlocalCase.Value ?? "";

                  break;
                case 2:
                  local.Current.Name = "CASXFRAP" + NumberToString
                    (local.CaseTransfer.Index, 15, 1);
                  local.FieldValue.Value =
                    local.CaseTransfer.Item.GlocalApName.Value ?? "";

                  break;
                default:
                  break;
              }

              UseSpCabCreateUpdateFieldValue2();

              if (IsExitState("DOCUMENT_FIELD_NF_RB"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

                return;
              }

              ++import.ExpImpRowLockFieldValue.Count;
            }
          }

          local.CaseTransfer.CheckIndex();

          break;
        default:
          // mjr
          // ----------------------------------------------
          // Field is a single value
          //    Process local field_value
          // -------------------------------------------------
          UseSpCabCreateUpdateFieldValue1();

          if (IsExitState("DOCUMENT_FIELD_NF_RB"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Creation Error";
            export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

            return;
          }

          ++import.ExpImpRowLockFieldValue.Count;

          break;
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      if (Equal(entities.Field.Name, "CASXFRAP0"))
      {
        local.PreviousField.Name = "CASXFRCAS9";
      }
      else
      {
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue1(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFieldValue2(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Current.Date = import.Current.Date;
    useImport.Case1.Number = local.Case1.Number;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateUpdateFieldValue1()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue2()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);
    useImport.Field.Name = local.Current.Name;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue3()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Address.Item.GlocalAddress.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private string UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase10()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase10",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.SpDocKey.KeyXferToDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyAr);
        db.SetDate(
          command, "effectiveDate",
          import.SpDocKey.KeyXferFromDate.GetValueOrDefault());
        db.SetInt32(command, "offId", import.SpDocKey.KeyOffice);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase11()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase11",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase12()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase12",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "adminAppealId", import.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase4()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase4",
      (db, command) =>
      {
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase5()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase6()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "testNumber", import.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase7()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase7",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.SpDocKey.KeyAppointment.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase8()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase8",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", export.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase9()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase9",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ArCaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.ArCaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.SpDocKey.KeyXferToDate.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Old.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.SpDocKey.KeyXferFromDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Old.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Old.Populated = true;
      });
  }

  private bool ReadOldNewXref()
  {
    entities.OldNewXref.Populated = false;

    return Read("ReadOldNewXref",
      (db, command) =>
      {
        db.SetString(command, "kessepCaseNbr", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.OldNewXref.KessepCaseNbr = db.GetString(reader, 0);
        entities.OldNewXref.CaecsesCaseNbr = db.GetString(reader, 1);
        entities.OldNewXref.ClientType = db.GetString(reader, 2);
        entities.OldNewXref.ClientNbr = db.GetInt64(reader, 3);
        entities.OldNewXref.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", local.Program.Code);
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Program.Code = db.GetString(reader, 6);
        entities.Program.EffectiveDate = db.GetDate(reader, 7);
        entities.Program.DiscontinueDate = db.GetDate(reader, 8);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private SpDocKey spDocKey;
    private Infrastructure infrastructure;
    private Document document;
    private Field field;
    private FieldValue fieldValue;
    private DateWorkArea current;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    private SpDocKey spDocKey;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CaseTransferGroup group.</summary>
    [Serializable]
    public class CaseTransferGroup
    {
      /// <summary>
      /// A value of GlocalCase.
      /// </summary>
      [JsonPropertyName("glocalCase")]
      public FieldValue GlocalCase
      {
        get => glocalCase ??= new();
        set => glocalCase = value;
      }

      /// <summary>
      /// A value of GlocalApName.
      /// </summary>
      [JsonPropertyName("glocalApName")]
      public FieldValue GlocalApName
      {
        get => glocalApName ??= new();
        set => glocalApName = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private FieldValue glocalCase;
      private FieldValue glocalApName;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of GlocalAddress.
      /// </summary>
      [JsonPropertyName("glocalAddress")]
      public FieldValue GlocalAddress
      {
        get => glocalAddress ??= new();
        set => glocalAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalAddress;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
    }

    /// <summary>
    /// Gets a value of CaseTransfer.
    /// </summary>
    [JsonIgnore]
    public Array<CaseTransferGroup> CaseTransfer => caseTransfer ??= new(
      CaseTransferGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseTransfer for json serialization.
    /// </summary>
    [JsonPropertyName("caseTransfer")]
    [Computed]
    public IList<CaseTransferGroup> CaseTransfer_Json
    {
      get => caseTransfer;
      set => CaseTransfer.Assign(value);
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
    }

    /// <summary>
    /// A value of PreviousField.
    /// </summary>
    [JsonPropertyName("previousField")]
    public Field PreviousField
    {
      get => previousField ??= new();
      set => previousField = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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

    /// <summary>
    /// A value of ProcessGroup.
    /// </summary>
    [JsonPropertyName("processGroup")]
    public Common ProcessGroup
    {
      get => processGroup ??= new();
      set => processGroup = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Field Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Field Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentRoot.
    /// </summary>
    [JsonPropertyName("currentRoot")]
    public Field CurrentRoot
    {
      get => currentRoot ??= new();
      set => currentRoot = value;
    }

    /// <summary>
    /// A value of Adc.
    /// </summary>
    [JsonPropertyName("adc")]
    public Common Adc
    {
      get => adc ??= new();
      set => adc = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
    private Case1 previousCase;
    private Array<CaseTransferGroup> caseTransfer;
    private PersonProgram personProgram;
    private Field field;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Program program;
    private Case1 case1;
    private DateWorkArea null1;
    private DateWorkArea zdelLocalCurrent;
    private Array<AddressGroup> address;
    private Field previousField;
    private FieldValue fieldValue;
    private DateWorkArea dateWorkArea;
    private Common processGroup;
    private Field temp;
    private Common position;
    private Field current;
    private Field currentRoot;
    private Common adc;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Office Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of OldNewXref.
    /// </summary>
    [JsonPropertyName("oldNewXref")]
    public OldNewXref OldNewXref
    {
      get => oldNewXref ??= new();
      set => oldNewXref = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    private Office old;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private CaseRole arCaseRole;
    private PersonProgram personProgram;
    private Program program;
    private OldNewXref oldNewXref;
    private InterstateRequest interstateRequest;
    private AdministrativeAppeal administrativeAppeal;
    private GeneticTest geneticTest;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private Appointment appointment;
  }
#endregion
}
