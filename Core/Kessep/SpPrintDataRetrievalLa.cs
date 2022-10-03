// Program: SP_PRINT_DATA_RETRIEVAL_LA, ID: 372132898, model: 746.
// Short name: SWE02232
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
/// A program: SP_PRINT_DATA_RETRIEVAL_LA.
/// </para>
/// <para>
/// This action block proposes to retreive all legal data required on all legal 
/// forms.
/// </para>
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalLa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_LA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalLa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalLa.
  /// </summary>
  public SpPrintDataRetrievalLa(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------
    // 10/01/1998	M Ramirez			Initial Development
    // 07/14/1999	M Ramirez			Added row lock counts
    // 06/15/2000	M Ramirez	80722		Service date of the most recent
    // 						legal action that has a class =
    // 						local legal_action class
    // 10/19/2016	GVandy		CQ54541		Add new field LACTKPCNUM.
    // ----------------------------------------------------------------------
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.SpDocKey.Assign(import.SpDocKey);

    // ------------------------------------------------------------
    // While we are here, process all fields on this document
    // that have this same dependancy.
    // ------------------------------------------------------------
    foreach(var item in ReadField())
    {
      // mjr--->  For Fields processed in this CAB
      if (Lt(entities.Field.SubroutineName, local.Previous.SubroutineName) || Equal
        (entities.Field.SubroutineName, local.Previous.SubroutineName) && !
        Lt(local.Previous.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case " DETERMN":
          if (import.SpDocKey.KeyLegalAction > 0)
          {
            local.LegalAction.Identifier = import.SpDocKey.KeyLegalAction;
          }
          else
          {
            switch(TrimEnd(entities.Field.Name))
            {
              case "CASHRCPTLA":
                if (!ReadCashReceiptDetail())
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                if (ReadLegalAction2())
                {
                  local.LegalAction.Identifier =
                    entities.LegalAction.Identifier;
                }

                if (local.LegalAction.Identifier <= 0)
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                if (!ReadLegalAction6())
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                break;
              case "GENTESTLA":
                if (ReadLegalAction4())
                {
                  local.LegalAction.Identifier =
                    entities.LegalAction.Identifier;
                }
                else
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                break;
              case "OBLIGATNLA":
                // ------------------------------------------------------------
                // Retrieve Legal Action from Obligation
                // ------------------------------------------------------------
                if (ReadLegalAction1())
                {
                  local.LegalAction.Identifier =
                    entities.LegalAction.Identifier;
                }
                else
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                break;
              case "WRKSHEETLA":
                // ------------------------------------------------------------
                // Retrieve Legal Action from Worksheet
                // ------------------------------------------------------------
                if (ReadLegalAction3())
                {
                  local.LegalAction.Identifier =
                    entities.LegalAction.Identifier;
                }
                else
                {
                  // mjr---> LA not found, but no message is given.
                  return;
                }

                break;
              default:
                export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
                export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                  (entities.Field.Name) + ",  Subroutine:  " + entities
                  .Field.SubroutineName;

                break;
            }
          }

          local.FieldValue.Value =
            NumberToString(local.LegalAction.Identifier, 7, 9);
          export.SpDocKey.KeyLegalAction = local.LegalAction.Identifier;

          break;
        case "LA":
          if (entities.LegalAction.Identifier <= 0)
          {
            // ------------------------------------------------------------
            // Verify that the Legal Action exists.
            // ------------------------------------------------------------
            if (!ReadLegalAction5())
            {
              // mjr---> LA not found, but no message is given.
              return;
            }
          }

          if (IsEmpty(entities.Tribunal.Name))
          {
            // ------------------------------------------------------------
            // Tribunal needs currency for some fields
            // ------------------------------------------------------------
            if (!ReadTribunal1())
            {
              return;
            }
          }

          if (Equal(entities.Field.Name, 1, 9, "LASERVCLS"))
          {
            local.Field.Name = "LASERVCLS*";
            local.LegalAction.Classification =
              Substring(entities.Field.Name, 10, 1);
            local.TempCommon.Count = 0;
          }
          else if (Equal(entities.Field.Name, 1, 9, "LATRBDHDR"))
          {
            local.Field.Name = "LATRBDHDR*";
            local.LegalAction.Classification = "";
            local.TempCommon.Count =
              (int)StringToNumber(Substring(
                entities.Field.Name, Field.Name_MaxLength, 10, 1));
          }
          else
          {
            local.Field.Name = entities.Field.Name;
            local.LegalAction.Classification = "";
            local.TempCommon.Count = 0;
          }

          switch(TrimEnd(local.Field.Name))
          {
            case "LACCAP01":
              local.ProcessGroup.Flag = "Y";
              local.Local1.Index = -1;

              foreach(var item1 in ReadCourtCaption())
              {
                ++local.Local1.Index;
                local.Local1.CheckSize();

                local.Local1.Update.GfieldValue.Value =
                  entities.CourtCaption.Line;

                if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity)
                {
                  break;
                }
              }

              break;
            case "LACTORDNUM":
              local.FieldValue.Value = entities.LegalAction.CourtCaseNumber;

              break;
            case "LACTSTDNUM":
              local.FieldValue.Value = entities.LegalAction.StandardNumber;

              break;
            case "LACTKPCNUM":
              // -- 10/19/16 GVandy  CQ54541  Add new document field LACTKPCNUM.
              // This field
              //    will contain the legal action standard with any blanks, 
              // backslashes(\), and
              //    asterisks(*) removed.
              local.TempFieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              local.Common.Count = 1;

              for(var limit =
                Length(TrimEnd(entities.LegalAction.StandardNumber)); local
                .Common.Count <= limit; ++local.Common.Count)
              {
                if (CharAt(entities.LegalAction.StandardNumber,
                  local.Common.Count) == '*' || CharAt
                  (entities.LegalAction.StandardNumber, local.Common.Count) == '\\'
                  || IsEmpty
                  (Substring(
                    entities.LegalAction.StandardNumber, local.Common.Count,
                  1)))
                {
                  // -- Continue
                }
                else
                {
                  local.TempFieldValue.Value =
                    TrimEnd(local.TempFieldValue.Value) + Substring
                    (entities.LegalAction.StandardNumber,
                    LegalAction.StandardNumber_MaxLength, local.Common.Count,
                    1);
                }
              }

              local.FieldValue.Value = local.TempFieldValue.Value ?? "";

              break;
            case "LAEIWO2EMP":
              // -- No processing is required for this field.  The field is 
              // populated with Y/N
              //    by the program triggering the document.  The value signifies
              // if the IWO document
              //    was sent electronically to the employer.
              continue;
            case "LAFILEDT01":
              // ------------------------------------------------------
              // LA Filed Date  ---  moved to LDET subroutine
              // ------------------------------------------------------
              break;
            case "LAFILEDT02":
              // ---------------------------------------------------------
              // LA Filed Date of the most recent legal action of COMPENIO
              // ---------------------------------------------------------
              if (ReadLegalAction7())
              {
                local.DateWorkArea.Date = entities.Temp.FiledDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "LAFILEDT03":
              // ---------------------------------------------------------
              // LA Filed Date of the most recent legal action of MWO
              // ---------------------------------------------------------
              // mlb - PR00259407 - 01/13/2006 - The following conditional was 
              // commented out
              // so that the new one would look for a character string of "
              // MWONOTHC" as well as "MWO".
              if (ReadLegalAction13())
              {
                local.DateWorkArea.Date = entities.Temp.FiledDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              // end
              break;
            case "LAFIPSCONM":
              if (ReadFips())
              {
                local.FieldValue.Value = entities.Fips.CountyDescription;
              }

              break;
            case "LAHRGDETLS":
              if (ReadHearing())
              {
                if (!Equal(entities.Hearing.ConductedDate, local.Null1.Date))
                {
                  local.DateWorkArea.Date = entities.Hearing.ConductedDate;
                  local.DateWorkArea.Time = entities.Hearing.ConductedTime;
                  local.FieldValue.Value = UseSpDocFormatHearingDateTime();
                }
              }

              break;
            case "LAPETNRNM":
              if (ReadCsePerson1())
              {
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

                  if (AsChar(import.Batch.Flag) == 'Y')
                  {
                    UseSiReadCsePersonBatch();

                    if (!IsEmpty(local.ReadCsePerson.Type1) && Equal
                      (local.ReadCsePerson.AdabasResponseCd, "0148"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Resource Error";
                      export.ErrorFieldValue.Value = "ADABAS Unavailable";
                    }
                  }
                  else
                  {
                    UseSiReadCsePerson();
                  }

                  if (!IsEmpty(local.ReadCsePerson.Type1) || !
                    IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    break;
                  }

                  local.SpPrintWorkSet.FirstName =
                    local.CsePersonsWorkSet.FirstName;
                  local.SpPrintWorkSet.MidInitial =
                    local.CsePersonsWorkSet.MiddleInitial;
                  local.SpPrintWorkSet.LastName =
                    local.CsePersonsWorkSet.LastName;
                  local.FieldValue.Value = UseSpDocFormatName();
                }
                else
                {
                  local.FieldValue.Value = entities.CsePerson.OrganizationName;
                }
              }

              break;
            case "LARESPONNM":
              if (ReadCsePerson2())
              {
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

                  if (AsChar(import.Batch.Flag) == 'Y')
                  {
                    UseSiReadCsePersonBatch();

                    if (!IsEmpty(local.ReadCsePerson.Type1) && Equal
                      (local.ReadCsePerson.AdabasResponseCd, "0148"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Resource Error";
                      export.ErrorFieldValue.Value = "ADABAS Unavailable";
                    }
                  }
                  else
                  {
                    UseSiReadCsePerson();
                  }

                  if (!IsEmpty(local.ReadCsePerson.Type1) || !
                    IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    break;
                  }

                  local.SpPrintWorkSet.FirstName =
                    local.CsePersonsWorkSet.FirstName;
                  local.SpPrintWorkSet.MidInitial =
                    local.CsePersonsWorkSet.MiddleInitial;
                  local.SpPrintWorkSet.LastName =
                    local.CsePersonsWorkSet.LastName;
                  local.FieldValue.Value = UseSpDocFormatName();
                }
                else
                {
                  local.FieldValue.Value = entities.CsePerson.OrganizationName;
                }
              }

              break;
            case "LASERVCLS*":
              // mjr
              // ---------------------------------------------------
              // 06/15/2000
              // PR# 80722 - Service date of the most recent legal action that
              // has a classification = local legal_action class
              // ---------------------------------------------------------------
              if (ReadLegalAction12())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LASERVDT01":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of DET1PATP or 
              // VOLPATPK.
              // ---------------------------------------------------------------
              if (ReadLegalAction14())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LASERVDT02":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of IWO.
              // ---------------------------------------------------------------
              if (ReadLegalAction9())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LASERVDT03":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of MWO.
              // ---------------------------------------------------------------
              // mlb - PR00259407 - 01/13/2006 - The following conditional was 
              // commented out
              // so that the new one would look for a character string of "
              // MWONOTHC" as well as "MWO".
              if (ReadLegalAction13())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              // end
              break;
            case "LASERVDT04":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of NOIIWON.
              // ---------------------------------------------------------------
              if (ReadLegalAction10())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LASERVDT05":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of IISSMWON.
              // ---------------------------------------------------------------
              if (ReadLegalAction8())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LASERVDT06":
              // ----------------------------------------------------------------
              // Service date of the most recent legal action of SUPPORTP.
              // ---------------------------------------------------------------
              if (ReadLegalAction11())
              {
                if (ReadServiceProcess())
                {
                  if (Lt(local.Null1.Date, entities.ServiceProcess.ServiceDate))
                  {
                    local.DateWorkArea.Date =
                      entities.ServiceProcess.ServiceDate;
                    local.FieldValue.Value = UseSpDocFormatDate();
                  }
                }
              }

              break;
            case "LATRBADDR1":
              local.ProcessGroup.Flag = "A";

              if (ReadFipsTribAddress1())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 = entities.FipsTribAddress.Street1;
                local.SpPrintWorkSet.Street2 =
                  entities.FipsTribAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.FipsTribAddress.City;
                local.SpPrintWorkSet.State = entities.FipsTribAddress.State;
                local.SpPrintWorkSet.ZipCode = entities.FipsTribAddress.ZipCode;
                local.SpPrintWorkSet.Zip4 = entities.FipsTribAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.FipsTribAddress.Zip3 ?? Spaces
                  (3);
                UseSpDocFormatAddress();
              }

              break;
            case "LATRBCITNM":
              if (IsEmpty(entities.FipsTribAddress.City))
              {
                ReadFipsTribAddress4();
              }

              local.FieldValue.Value = entities.FipsTribAddress.City;

              break;
            case "LATRBDHDR*":
              switch(local.TempCommon.Count)
              {
                case 1:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader1;

                  break;
                case 2:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader2;

                  break;
                case 3:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader3;

                  break;
                case 4:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader4;

                  break;
                case 5:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader5;

                  break;
                case 6:
                  local.FieldValue.Value = entities.Tribunal.DocumentHeader6;

                  break;
                default:
                  break;
              }

              break;
            case "LATRBNM":
              local.FieldValue.Value = entities.Tribunal.Name;

              break;
            case "LATRBPYAD1":
              local.ProcessGroup.Flag = "A";

              if (AsChar(entities.LegalAction.CtOrdAltBillingAddrInd) == 'Y')
              {
                if (ReadTribunal2())
                {
                  if (ReadFipsTribAddress3())
                  {
                    local.SpPrintWorkSet.LocationType = "D";
                    local.SpPrintWorkSet.Street1 =
                      entities.FipsTribAddress.Street1;
                    local.SpPrintWorkSet.Street2 =
                      entities.FipsTribAddress.Street2 ?? Spaces(25);
                    local.SpPrintWorkSet.City = entities.FipsTribAddress.City;
                    local.SpPrintWorkSet.State = entities.FipsTribAddress.State;
                    local.SpPrintWorkSet.ZipCode =
                      entities.FipsTribAddress.ZipCode;
                    local.SpPrintWorkSet.Zip4 =
                      entities.FipsTribAddress.Zip4 ?? Spaces(4);
                    local.SpPrintWorkSet.Zip3 =
                      entities.FipsTribAddress.Zip3 ?? Spaces(3);
                    UseSpDocFormatAddress();
                  }
                }
              }
              else if (ReadFipsTribAddress1())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 = entities.FipsTribAddress.Street1;
                local.SpPrintWorkSet.Street2 =
                  entities.FipsTribAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.FipsTribAddress.City;
                local.SpPrintWorkSet.State = entities.FipsTribAddress.State;
                local.SpPrintWorkSet.ZipCode = entities.FipsTribAddress.ZipCode;
                local.SpPrintWorkSet.Zip4 = entities.FipsTribAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.FipsTribAddress.Zip3 ?? Spaces
                  (3);
                UseSpDocFormatAddress();
              }

              break;
            case "LATRBPYNM":
              if (AsChar(entities.LegalAction.CtOrdAltBillingAddrInd) == 'Y')
              {
                if (ReadTribunal2())
                {
                  local.FieldValue.Value = entities.PayTribunal.Name;
                }
              }
              else
              {
                local.FieldValue.Value = entities.Tribunal.Name;
              }

              break;
            case "LATRBSADD1":
              local.ProcessGroup.Flag = "A";

              if (ReadFipsTribAddress2())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 = entities.FipsTribAddress.Street1;
                local.SpPrintWorkSet.Street2 =
                  entities.FipsTribAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.FipsTribAddress.City;
                local.SpPrintWorkSet.State = entities.FipsTribAddress.State;
                local.SpPrintWorkSet.ZipCode = entities.FipsTribAddress.ZipCode;
                local.SpPrintWorkSet.Zip4 = entities.FipsTribAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.FipsTribAddress.Zip3 ?? Spaces
                  (3);
                UseSpDocFormatAddress();
              }

              break;
            case "LATRBSTNM":
              if (IsEmpty(entities.FipsTribAddress.State))
              {
                ReadFipsTribAddress4();
              }

              local.ValidateCode.CodeName = "STATE CODE";
              local.ValidateCodeValue.Cdvalue = entities.FipsTribAddress.State;
              UseCabGetCodeValueDescription();
              local.FieldValue.Value = local.ValidateCodeValue.Description;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "LDET DTL":
          // mjr--->  For Fields processed in called CABs
          if (!Lt(local.Previous.SubroutineName, entities.Field.SubroutineName))
          {
            continue;
          }

          local.ProcessGroup.Flag = "";
          UseSpPrintDataRetrievalLdet();

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
          //    Process 1-5 of group_local
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
            local.TempField.Name = NumberToString(local.Address.Index + 1, 10);
            local.Position.Count = Verify(local.TempField.Name, "0");
            local.TempField.Name =
              Substring(local.TempField.Name, local.Position.Count, 16 -
              local.Position.Count);
            local.Current.Name = TrimEnd(local.CurrentRoot.Name) + local
              .TempField.Name;
            UseSpCabCreateUpdateFieldValue2();

            if (IsExitState("DOCUMENT_FIELD_NF_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.ErrorDocumentField.ScreenPrompt = "Creation Error";
              export.ErrorFieldValue.Value = "Field:  " + local.Current.Name;

              return;
            }

            local.Address.Update.GlocalAddress.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Address.CheckIndex();

          break;
        case 'Y':
          // mjr
          // ----------------------------------------------
          // Field is an regular group
          //    Process 1-12 of group_local  (Field names 01-12)
          // -------------------------------------------------
          local.Position.Count = Length(TrimEnd(entities.Field.Name));
          local.CurrentRoot.Name =
            Substring(entities.Field.Name, 1, local.Position.Count - 2);

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            // mjr---> Increment Field Name
            local.TempField.Name = NumberToString(local.Local1.Index + 1, 10);
            local.Position.Count = Verify(local.TempField.Name, "0");
            local.TempField.Name =
              Substring(local.TempField.Name, local.Position.Count, 16 -
              local.Position.Count);

            if (local.Local1.Index < 9)
            {
              local.Current.Name = TrimEnd(local.CurrentRoot.Name) + "0";
            }
            else
            {
              local.Current.Name = local.CurrentRoot.Name;
            }

            local.Current.Name = TrimEnd(local.Current.Name) + local
              .TempField.Name;
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

            local.Local1.Update.GfieldValue.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Local1.CheckIndex();

          break;
        case ' ':
          // mjr
          // -----------------------------------------------
          // This field was handled in a subroutine
          // --------------------------------------------------
          break;
        default:
          // mjr
          // ----------------------------------------------
          // Field is a single value
          //    Process local field_value
          // -------------------------------------------------
          // --------------------------------------------------------
          // Add Field_value
          // Need to update PAD to include doc_field
          // (which means document and field need to be imported)
          // --------------------------------------------------------
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

      MoveField1(entities.Field, local.Previous);

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      switch(TrimEnd(entities.Field.Name))
      {
        case "LACCAP01":
          local.Previous.Name = "LACCAP99";

          break;
        case "LATRBADDR1":
          local.Previous.Name = "LATRBADDR5";

          break;
        case "LATRBPYAD1":
          local.Previous.Name = "LATRBPYAD5";

          break;
        case "LATRBSADD1":
          local.Previous.Name = "LATRBSADD5";

          break;
        default:
          break;
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveExport1ToAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.AddressGroup target)
  {
    target.GlocalAddress.Value = source.G.Value;
  }

  private static void MoveField1(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveField2(Field source, Field target)
  {
    target.Dependancy = source.Dependancy;
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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
    target.KeyLegalAction = source.KeyLegalAction;
  }

  private static void MoveSpPrintWorkSet(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MidInitial = source.MidInitial;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;
    useImport.Code.CodeName = local.ValidateCode.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.ValidateCodeValue);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReadCsePerson.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.ReadCsePerson.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Address.Item.GlocalAddress.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue3()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Local1.Item.GfieldValue.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpDocFormatAddress()
  {
    var useImport = new SpDocFormatAddress.Import();
    var useExport = new SpDocFormatAddress.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatAddress.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Address, MoveExport1ToAddress);
  }

  private string UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatHearingDateTime()
  {
    var useImport = new SpDocFormatHearingDateTime.Import();
    var useExport = new SpDocFormatHearingDateTime.Export();

    MoveDateWorkArea(local.DateWorkArea, useImport.DateWorkArea);

    Call(SpDocFormatHearingDateTime.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    MoveSpPrintWorkSet(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private void UseSpPrintDataRetrievalLdet()
  {
    var useImport = new SpPrintDataRetrievalLdet.Import();
    var useExport = new SpPrintDataRetrievalLdet.Export();

    MoveFieldValue2(import.FieldValue, useImport.FieldValue);
    MoveDocument(import.Document, useImport.Document);
    MoveField2(entities.Field, useImport.Field);
    MoveInfrastructure(import.Infrastructure, useImport.Infrastructure);
    MoveSpDocKey(export.SpDocKey, useImport.SpDocKey);
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalLdet.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    export.ErrorInd.Flag = useExport.ErrorInd.Flag;
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.SpDocKey.KeyCashRcptDetail);
        db.SetInt32(command, "crtIdentifier", import.SpDocKey.KeyCashRcptType);
        db.SetInt32(command, "crvIdentifier", import.SpDocKey.KeyCashRcptEvent);
        db.
          SetInt32(command, "cstIdentifier", import.SpDocKey.KeyCashRcptSource);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCourtCaption()
  {
    entities.CourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 10);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 10);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress3()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.PayTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 10);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress4()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 10);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "obId", import.SpDocKey.KeyObligation);
        db.
          SetInt32(command, "dtyGeneratedId", import.SpDocKey.KeyObligationType);
          
        db.SetString(command, "cspNumber", import.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction10()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction10",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction11()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction11",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction12()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction12",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.
          SetString(command, "classification", local.LegalAction.Classification);
          
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction13()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction13",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction14()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction14",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt64(command, "identifier", import.SpDocKey.KeyWorksheet);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction4()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction4",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", import.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction5()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction5",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction6()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction6",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction7()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction7",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction8()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction8",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadLegalAction9()
  {
    entities.Temp.Populated = false;

    return Read("ReadLegalAction9",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Temp.Identifier = db.GetInt32(reader, 0);
        entities.Temp.Classification = db.GetString(reader, 1);
        entities.Temp.ActionTaken = db.GetString(reader, 2);
        entities.Temp.FiledDate = db.GetNullableDate(reader, 3);
        entities.Temp.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Temp.TrbId = db.GetNullableInt32(reader, 5);
        entities.Temp.Populated = true;
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.ServiceProcess.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.DocumentHeader1 = db.GetNullableString(reader, 3);
        entities.Tribunal.DocumentHeader2 = db.GetNullableString(reader, 4);
        entities.Tribunal.DocumentHeader3 = db.GetNullableString(reader, 5);
        entities.Tribunal.DocumentHeader4 = db.GetNullableString(reader, 6);
        entities.Tribunal.DocumentHeader5 = db.GetNullableString(reader, 7);
        entities.Tribunal.DocumentHeader6 = db.GetNullableString(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.PayTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.PayTribunal.Name = db.GetString(reader, 0);
        entities.PayTribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.PayTribunal.Identifier = db.GetInt32(reader, 2);
        entities.PayTribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.PayTribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.PayTribunal.Populated = true;
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    private Common batch;
    private Infrastructure infrastructure;
    private Field field;
    private Document document;
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
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
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
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    private SpDocKey spDocKey;
    private FieldValue errorFieldValue;
    private DocumentField errorDocumentField;
    private Common errorInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet gcsePersonsWorkSet;
      private FieldValue gfieldValue;
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
    /// A value of TempFieldValue.
    /// </summary>
    [JsonPropertyName("tempFieldValue")]
    public FieldValue TempFieldValue
    {
      get => tempFieldValue ??= new();
      set => tempFieldValue = value;
    }

    /// <summary>
    /// A value of TempCommon.
    /// </summary>
    [JsonPropertyName("tempCommon")]
    public Common TempCommon
    {
      get => tempCommon ??= new();
      set => tempCommon = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Textnum.
    /// </summary>
    [JsonPropertyName("textnum")]
    public WorkArea Textnum
    {
      get => textnum ??= new();
      set => textnum = value;
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
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
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of TempField.
    /// </summary>
    [JsonPropertyName("tempField")]
    public Field TempField
    {
      get => tempField ??= new();
      set => tempField = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public SpDocKey Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Common common;
    private FieldValue tempFieldValue;
    private Common tempCommon;
    private Field field;
    private Array<AddressGroup> address;
    private FieldValue fieldValue;
    private Field previous;
    private LegalAction legalAction;
    private DateWorkArea null1;
    private WorkArea textnum;
    private Infrastructure infrastructure;
    private SpPrintWorkSet spPrintWorkSet;
    private Common adc;
    private CodeValue validateCodeValue;
    private Code validateCode;
    private DateWorkArea dateWorkArea;
    private AbendData readCsePerson;
    private Array<LocalGroup> local1;
    private Common processGroup;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Field tempField;
    private Common position;
    private Field current;
    private Field currentRoot;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private ChildSupportWorksheet childSupportWorksheet;
    private DateWorkArea zdelLocalCurrent;
    private SpDocKey zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PayFips.
    /// </summary>
    [JsonPropertyName("payFips")]
    public Fips PayFips
    {
      get => payFips ??= new();
      set => payFips = value;
    }

    /// <summary>
    /// A value of PayTribunal.
    /// </summary>
    [JsonPropertyName("payTribunal")]
    public Tribunal PayTribunal
    {
      get => payTribunal ??= new();
      set => payTribunal = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public LegalAction Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    private Fips payFips;
    private Tribunal payTribunal;
    private GeneticTest geneticTest;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ChildSupportWorksheet childSupportWorksheet;
    private ServiceProcess serviceProcess;
    private LegalAction temp;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private ObligationType obligationType;
    private Case1 case1;
    private Fips fips;
    private LegalActionDetail legalActionDetail;
    private Hearing hearing;
    private FipsTribAddress fipsTribAddress;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private Tribunal tribunal;
    private CourtCaption courtCaption;
    private LegalAction legalAction;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
  }
#endregion
}
