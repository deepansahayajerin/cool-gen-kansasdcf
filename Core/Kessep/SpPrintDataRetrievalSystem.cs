// Program: SP_PRINT_DATA_RETRIEVAL_SYSTEM, ID: 372132896, model: 746.
// Short name: SWE02231
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_SYSTEM.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalSystem: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_SYSTEM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalSystem(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalSystem.
  /// </summary>
  public SpPrintDataRetrievalSystem(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------
    // 10/02/1998	M Ramirez			Initial Development
    // 07/14/1999	M Ramirez			Added row lock counts
    // 03/03/2001	M Ramirez	WR291 Seg C	Added locate request
    // 10/15/2001	M Ramirez	WR10533		Added office fax and sp eMail
    // 07/31/2003	GVandy 		WR040138  	Garden City tiered service provider pilot.
    // 12/02/2004	M J Quinn	WR040802
    // Expanded the Garden City Customer Service Center pilot.  Modules 
    // SWE02231, SWEOFFCS, SWEOFFCP,
    // SWE01441, SWE01311, SWE00091 are included in this work request.
    // 02/20/2008	M Ramirez	WR 276, 277	Added SYSLITRL
    // 11/17/2011	GVandy		CQ30161		Use current_date plus one day when finding 
    // service provider for CASETXFR document.
    // ----------------------------------------------------------------------
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.Infrastructure.UserId = import.Infrastructure.UserId;
    UseCabSetMaximumDiscontinueDate();
    local.CurrentPlus1.Date = AddDays(import.Current.Date, 1);

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
      MoveField(entities.Field, local.Previous);

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case "SERVPROV":
          // mjr
          // -------------------------------------------------
          // 01/12/1999
          // The normal method for detemrining the SP uses the fields:
          // 	SPADDR1, SPNAME, SPOFFCCONM, etc.
          // The previous SP assigned to the CAS, uses the fields:
          // 	SPPRVADDR1, SPPRVNAME, SPPRVPHONE.
          // The SP assigned to the LEA, uses the fields:
          // 	SP2ADDR1, SP2NAME.
          // --------------------------------------------------------------
          if (Equal(entities.Field.Name, "SPPRVADDR1") || Equal
            (entities.Field.Name, "SPPRVNAME") || Equal
            (entities.Field.Name, "SPPRVPHONE"))
          {
            if (IsEmpty(local.Alternate.ServProvUserId))
            {
              foreach(var item1 in ReadCaseAssignment())
              {
                // mjr
                // --------------------------------------------------------
                // Using discontinue_date < max_date gives the latest assignment
                // that has been ended.
                // -----------------------------------------------------------
                if (ReadOfficeServiceProviderOfficeServiceProvider())
                {
                  local.Alternate.ServProvUserId =
                    entities.ServiceProvider.UserId;
                  local.Alternate.ServProvFirstName =
                    entities.ServiceProvider.FirstName;
                  local.Alternate.ServProvMi =
                    entities.ServiceProvider.MiddleInitial;
                  local.Alternate.ServProvLastName =
                    entities.ServiceProvider.LastName;
                  local.Alternate.OspCertificationNumber =
                    entities.ServiceProvider.CertificationNumber;
                  local.Alternate.OspRoleCode =
                    entities.OfficeServiceProvider.RoleCode;
                  local.Alternate.OspLocalContactCode =
                    entities.OfficeServiceProvider.LocalContactCodeForIrs;
                  local.Alternate.OspWorkPhoneAreaCode =
                    entities.OfficeServiceProvider.WorkPhoneAreaCode;
                  local.Alternate.OspWorkPhoneNumber =
                    entities.OfficeServiceProvider.WorkPhoneNumber;
                  local.Alternate.OspWorkPhoneExtension =
                    entities.OfficeServiceProvider.WorkPhoneExtension;
                  local.Alternate.OfficeSysGenId =
                    entities.Office.SystemGeneratedId;
                  local.Alternate.OfficeName = entities.Office.Name;
                }
                else
                {
                  break;
                }

                if (ReadServiceProviderAddress())
                {
                  local.Alternate.OffcAddrStreet1 =
                    entities.ServiceProviderAddress.Street1;
                  local.Alternate.OffcAddrStreet2 =
                    entities.ServiceProviderAddress.Street2;
                  local.Alternate.OffcAddrCity =
                    entities.ServiceProviderAddress.City;
                  local.Alternate.OffcAddrState =
                    entities.ServiceProviderAddress.StateProvince;
                  local.Alternate.OffcAddrZip =
                    entities.ServiceProviderAddress.Zip;
                  local.Alternate.OffcAddrZip4 =
                    entities.ServiceProviderAddress.Zip4;
                  local.Alternate.OffcAddrZip3 =
                    entities.ServiceProviderAddress.Zip3;

                  break;
                }

                if (ReadOfficeAddress())
                {
                  local.Alternate.OffcAddrStreet1 =
                    entities.OfficeAddress.Street1;
                  local.Alternate.OffcAddrStreet2 =
                    entities.OfficeAddress.Street2;
                  local.Alternate.OffcAddrCity = entities.OfficeAddress.City;
                  local.Alternate.OffcAddrState =
                    entities.OfficeAddress.StateProvince;
                  local.Alternate.OffcAddrZip = entities.OfficeAddress.Zip;
                  local.Alternate.OffcAddrZip4 = entities.OfficeAddress.Zip4;
                  local.Alternate.OffcAddrZip3 = entities.OfficeAddress.Zip3;

                  break;
                }
              }

              if (IsEmpty(entities.OfficeServiceProvider.RoleCode))
              {
                // mjr---> To skip SERVPROV fields
                local.Previous.SubroutineName = "SERVPROW";

                continue;
              }
            }
          }
          else if (Equal(entities.Field.Name, "SP2ADDR1") || Equal
            (entities.Field.Name, "SP2NAME"))
          {
            if (IsEmpty(local.Alternate.ServProvUserId))
            {
              local.Document.BusinessObject = "LRF";
              UseSpDocGetServiceProvider3();

              if (IsEmpty(local.Alternate.ServProvUserId))
              {
                // mjr---> To skip SERVPROV fields
                local.Previous.SubroutineName = "SERVPROW";

                continue;
              }
            }
          }
          else if (IsEmpty(local.OutDocRtrnAddr.ServProvUserId))
          {
            if (Equal(import.Document.Name, "CASETXFR"))
            {
              // 11/17/2011  GVandy  CQ30161  Use current_date plus one day when
              // finding service provider for CASETXFR document.
              UseSpDocGetServiceProvider2();
            }
            else
            {
              UseSpDocGetServiceProvider1();
            }

            if (IsEmpty(local.OutDocRtrnAddr.ServProvUserId))
            {
              if (Equal(import.Document.BusinessObject, "OAA"))
              {
                local.Document.BusinessObject = "PER";
                UseSpDocGetServiceProvider4();
              }
              else if (Equal(import.Document.Name, "POSTMAST"))
              {
                local.Document.BusinessObject = "OBL";
                UseSpDocGetServiceProvider4();
              }
              else
              {
              }
            }

            if (IsEmpty(local.OutDocRtrnAddr.ServProvUserId))
            {
              // mjr---> To skip SERVPROV fields
              local.Previous.SubroutineName = "SERVPROW";

              continue;
            }

            export.Infrastructure.UserId = local.OutDocRtrnAddr.ServProvUserId;
          }

          // 7/31/03 GVandy WR040138  Garden City tiered service provider pilot.
          // The following offices are located in the Garden City area:
          //  Dodge City  (3)
          //  Garden City (5)
          //  Pratt (17)
          //  Liberal (26)
          //  Ulysses (29)
          // For these offices the service providers name should be replaced 
          // with "Customer Service Representative".  The address should be
          // replaced with the mailing address of the office to which the
          // service provider is assigned.  The phone number should be replaced
          // with  the phone number for the Garden City office.
          // A list of documents excluded from this process are included in code
          // table "Excluded Tier 1 Documents"
          if (IsEmpty(local.Tier1ServiceProvider.Flag))
          {
            local.Tier1ServiceProvider.Flag = "N";
            ReadOffice1();

            if (ReadOffice2())
            {
              // Check if the document is excluded from this process...
              local.CodeValue.Cdvalue = import.Document.Name;

              if (ReadCodeValue())
              {
                // -- The document is excluded from the tiered service provider 
                // pilot.
                goto Test;
              }

              local.Tier1ServiceProvider.Flag = "Y";

              // Read the mailing address for the office to which the service 
              // provider is assigned...
              if (ReadOfficeOfficeAddress())
              {
                local.OutDocRtrnAddr.OffcAddrStreet1 =
                  entities.Tier1OfficeAddress.Street1;
                local.OutDocRtrnAddr.OffcAddrStreet2 =
                  entities.Tier1OfficeAddress.Street2;
                local.OutDocRtrnAddr.OffcAddrCity =
                  entities.Tier1OfficeAddress.City;
                local.OutDocRtrnAddr.OffcAddrState =
                  entities.Tier1OfficeAddress.StateProvince;
                local.OutDocRtrnAddr.OffcAddrZip =
                  entities.Tier1OfficeAddress.Zip;
                local.OutDocRtrnAddr.OffcAddrZip3 =
                  entities.Tier1OfficeAddress.Zip3;
                local.OutDocRtrnAddr.OffcAddrZip4 =
                  entities.Tier1OfficeAddress.Zip4;
                local.Office.MainFaxAreaCode =
                  entities.Tier1Office.MainFaxAreaCode;
                local.Office.MainFaxPhoneNumber =
                  entities.Tier1Office.MainFaxPhoneNumber;
              }

              // Use the Customer Service Center office phone number instead of 
              // the local office phone number
              local.OutDocRtrnAddr.OspWorkPhoneAreaCode =
                entities.CustomerServiceCenter.MainPhoneAreaCode.
                  GetValueOrDefault();
              local.OutDocRtrnAddr.OspWorkPhoneNumber =
                entities.CustomerServiceCenter.MainPhoneNumber.
                  GetValueOrDefault();
              local.OutDocRtrnAddr.OspWorkPhoneExtension = "";
            }
          }

Test:

          switch(TrimEnd(entities.Field.Name))
          {
            case "SPADDR1":
              local.ProcessGroup.Flag = "A";

              if (!IsEmpty(local.OutDocRtrnAddr.OffcAddrStreet1))
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 =
                  local.OutDocRtrnAddr.OffcAddrStreet1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  local.OutDocRtrnAddr.OffcAddrStreet2 ?? Spaces(25);
                local.SpPrintWorkSet.City =
                  local.OutDocRtrnAddr.OffcAddrCity ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  local.OutDocRtrnAddr.OffcAddrState ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  local.OutDocRtrnAddr.OffcAddrZip ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  local.OutDocRtrnAddr.OffcAddrZip4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  local.OutDocRtrnAddr.OffcAddrZip3 ?? Spaces(3);
                UseSpDocFormatAddress();
              }

              break;
            case "SPEMAIL":
              local.FieldValue.Value = local.ServiceProvider.EmailAddress ?? "";

              break;
            case "SPFAX":
              local.SpPrintWorkSet.PhoneAreaCode =
                local.Office.MainFaxAreaCode.GetValueOrDefault();
              local.SpPrintWorkSet.Phone7Digit =
                local.Office.MainFaxPhoneNumber.GetValueOrDefault();
              local.SpPrintWorkSet.PhoneExt = "";
              local.FieldValue.Value = UseSpDocFormatPhoneNumber();

              break;
            case "SPNAME":
              if (AsChar(local.Tier1ServiceProvider.Flag) == 'Y')
              {
                local.FieldValue.Value = "CUSTOMER SERVICE REPRESENTATIVE";
              }
              else
              {
                local.SpPrintWorkSet.FirstName =
                  local.OutDocRtrnAddr.ServProvFirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.OutDocRtrnAddr.ServProvMi ?? Spaces(1);
                local.SpPrintWorkSet.LastName =
                  local.OutDocRtrnAddr.ServProvLastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }

              break;
            case "SPOFFCCONM":
              if (local.OutDocRtrnAddr.OfficeSysGenId > 0)
              {
                if (ReadCseOrganization())
                {
                  local.FieldValue.Value = entities.CseOrganization.Name;
                }
              }

              break;
            case "SPOFFICENM":
              local.FieldValue.Value = local.OutDocRtrnAddr.OfficeName;

              break;
            case "SPPHONE":
              local.SpPrintWorkSet.PhoneAreaCode =
                local.OutDocRtrnAddr.OspWorkPhoneAreaCode;
              local.SpPrintWorkSet.Phone7Digit =
                local.OutDocRtrnAddr.OspWorkPhoneNumber;
              local.SpPrintWorkSet.PhoneExt =
                local.OutDocRtrnAddr.OspWorkPhoneExtension ?? Spaces(5);
              local.FieldValue.Value = UseSpDocFormatPhoneNumber();

              break;
            case "SPPRVADDR1":
              local.ProcessGroup.Flag = "A";

              if (!IsEmpty(local.Alternate.OffcAddrStreet1))
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 =
                  local.Alternate.OffcAddrStreet1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  local.Alternate.OffcAddrStreet2 ?? Spaces(25);
                local.SpPrintWorkSet.City = local.Alternate.OffcAddrCity ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = local.Alternate.OffcAddrState ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode = local.Alternate.OffcAddrZip ?? Spaces
                  (5);
                local.SpPrintWorkSet.Zip4 = local.Alternate.OffcAddrZip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = local.Alternate.OffcAddrZip3 ?? Spaces
                  (3);
                UseSpDocFormatAddress();
              }

              break;
            case "SPPRVNAME":
              if (AsChar(local.Tier1ServiceProvider.Flag) == 'Y')
              {
                local.FieldValue.Value = "CUSTOMER SERVICE REPRESENTATIVE";
              }
              else
              {
                local.SpPrintWorkSet.FirstName =
                  local.Alternate.ServProvFirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.Alternate.ServProvMi ?? Spaces(1);
                local.SpPrintWorkSet.LastName =
                  local.Alternate.ServProvLastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }

              break;
            case "SPPRVPHONE":
              local.SpPrintWorkSet.PhoneAreaCode =
                local.Alternate.OspWorkPhoneAreaCode;
              local.SpPrintWorkSet.Phone7Digit =
                local.Alternate.OspWorkPhoneNumber;
              local.SpPrintWorkSet.PhoneExt =
                local.Alternate.OspWorkPhoneExtension ?? Spaces(5);
              local.FieldValue.Value = UseSpDocFormatPhoneNumber();

              break;
            case "SPSCNO":
              local.FieldValue.Value =
                local.OutDocRtrnAddr.OspCertificationNumber ?? "";

              break;
            case "SPTITLE":
              if (AsChar(local.Tier1ServiceProvider.Flag) == 'Y')
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }
              else
              {
                local.ValidateCode.CodeName = "OFFICE SERVICE PROVIDER ROLE";
                local.ValidateCodeValue.Cdvalue =
                  local.OutDocRtrnAddr.OspRoleCode;
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.ValidateCodeValue.Description;
              }

              break;
            case "SP2ADDR1":
              local.ProcessGroup.Flag = "A";
              local.SpPrintWorkSet.LocationType = "D";
              local.SpPrintWorkSet.Street1 =
                local.Alternate.OffcAddrStreet1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                local.Alternate.OffcAddrStreet2 ?? Spaces(25);
              local.SpPrintWorkSet.City = local.Alternate.OffcAddrCity ?? Spaces
                (15);
              local.SpPrintWorkSet.State = local.Alternate.OffcAddrState ?? Spaces
                (2);
              local.SpPrintWorkSet.ZipCode = local.Alternate.OffcAddrZip ?? Spaces
                (5);
              local.SpPrintWorkSet.Zip4 = local.Alternate.OffcAddrZip4 ?? Spaces
                (4);
              local.SpPrintWorkSet.Zip3 = local.Alternate.OffcAddrZip3 ?? Spaces
                (3);
              UseSpDocFormatAddress();

              break;
            case "SP2NAME":
              if (AsChar(local.Tier1ServiceProvider.Flag) == 'Y')
              {
                local.FieldValue.Value = "CUSTOMER SERVICE REPRESENTATIVE";
              }
              else
              {
                local.SpPrintWorkSet.FirstName =
                  local.Alternate.ServProvFirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.Alternate.ServProvMi ?? Spaces(1);
                local.SpPrintWorkSet.LastName =
                  local.Alternate.ServProvLastName;
                local.FieldValue.Value = UseSpDocFormatName();
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
        case "SYSINPUT":
          // mjr
          // -------------------------------------------------
          // This field is created by a process at the time that
          // the outgoing_document is created, so should already
          // be populated.  No action is necessary.
          // ----------------------------------------------------
          continue;
        case "SYSLITRL":
          // mjr
          // -------------------------------------------------
          // SYSLITRL is for short but static text
          // ----------------------------------------------------
          local.ValidateCode.CodeName = "DOCUMENT SYSLITRL";
          local.ValidateCodeValue.Cdvalue = entities.Field.Name;
          UseCabGetCodeValueDescription();
          local.FieldValue.Value = local.ValidateCodeValue.Description;

          break;
        case "SYSTEM":
          if (Equal(entities.Field.Name, 1, 7, "CURRDT+"))
          {
            local.Field.Name = "CURRDT+*";
            local.BatchConvertNumToText.TextNumber15 =
              Substring(entities.Field.Name, 8, 3);

            if (!Lt(local.BatchConvertNumToText.TextNumber15, "0") && !
              Lt("999", local.BatchConvertNumToText.TextNumber15))
            {
              local.BatchConvertNumToText.Number15 =
                StringToNumber(local.BatchConvertNumToText.TextNumber15);
            }
            else
            {
              local.BatchConvertNumToText.Number15 = 0;
            }
          }
          else
          {
            local.Field.Name = entities.Field.Name;
            local.BatchConvertNumToText.Number15 = 0;
          }

          switch(TrimEnd(local.Field.Name))
          {
            case "SYSCURRDT":
              local.DateWorkArea.Date = import.Current.Date;
              local.FieldValue.Value = UseSpDocFormatDate();

              break;
            case "CURRDT+*":
              local.DateWorkArea.Date =
                AddDays(import.Current.Date,
                (int)local.BatchConvertNumToText.Number15);
              local.FieldValue.Value = UseSpDocFormatDate();

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "USRINPUT":
          // mjr
          // -------------------------------------------------
          // This field is USER INPUT.  No action is necessary.
          // ----------------------------------------------------
          continue;
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

      if (AsChar(local.ProcessGroup.Flag) == 'A')
      {
        // mjr
        // ----------------------------------------------
        // Field is an address
        //    Process 1-5 of group_local
        // -------------------------------------------------
        local.Position.Count = Length(TrimEnd(entities.Field.Name));
        local.CurrentRoot.Name =
          Substring(entities.Field.Name, 1, local.Position.Count - 1);

        for(local.Address.Index = 0; local.Address.Index < local.Address.Count; ++
          local.Address.Index)
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

          local.Address.Update.GlocalAddress.Value =
            Spaces(FieldValue.Value_MaxLength);
          ++import.ExpImpRowLockFieldValue.Count;
        }

        local.Address.CheckIndex();
      }
      else
      {
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
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      switch(TrimEnd(entities.Field.Name))
      {
        case "SPADDR1":
          local.Previous.Name = "SPADDR5";

          break;
        case "SPPRVADDR1":
          local.Previous.Name = "SPPRVADDR5";

          break;
        case "SP2ADDR1":
          local.Previous.Name = "SP2ADDR5";

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

  private static void MoveExport1ToAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.AddressGroup target)
  {
    target.GlocalAddress.Value = source.G.Value;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.MainFaxAreaCode = source.MainFaxAreaCode;
    target.MainFaxPhoneNumber = source.MainFaxPhoneNumber;
  }

  private static void MoveSpPrintWorkSet1(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Phone7Digit = source.Phone7Digit;
  }

  private static void MoveSpPrintWorkSet2(SpPrintWorkSet source,
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

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.ValidateCodeValue);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
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

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    MoveSpPrintWorkSet2(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatPhoneNumber()
  {
    var useImport = new SpDocFormatPhoneNumber.Import();
    var useExport = new SpDocFormatPhoneNumber.Export();

    MoveSpPrintWorkSet1(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatPhoneNumber.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private void UseSpDocGetServiceProvider1()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.ServiceProvider.EmailAddress = useExport.ServiceProvider.EmailAddress;
    MoveOffice(useExport.Office, local.Office);
    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
  }

  private void UseSpDocGetServiceProvider2()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = local.CurrentPlus1.Date;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
    local.ServiceProvider.EmailAddress = useExport.ServiceProvider.EmailAddress;
    MoveOffice(useExport.Office, local.Office);
  }

  private void UseSpDocGetServiceProvider3()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    useImport.Document.BusinessObject = local.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.Alternate.Assign(useExport.OutDocRtrnAddr);
  }

  private void UseSpDocGetServiceProvider4()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    useImport.Document.BusinessObject = local.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
    local.ServiceProvider.EmailAddress = useExport.ServiceProvider.EmailAddress;
    MoveOffice(useExport.Office, local.Office);
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNo", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetString(command, "cdvalue", local.CodeValue.Cdvalue);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.OutDocRtrnAddr.OfficeSysGenId);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.Populated = true;
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

  private bool ReadOffice1()
  {
    entities.ClientOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.OutDocRtrnAddr.OfficeSysGenId);
      },
      (db, reader) =>
      {
        entities.ClientOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ClientOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ClientOffice.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    System.Diagnostics.Debug.Assert(entities.ClientOffice.Populated);
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.ClientOffice.OffOffice.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.MainPhoneNumber =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 3);
        entities.CustomerServiceCenter.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 4);
        entities.CustomerServiceCenter.MainFaxAreaCode =
          db.GetNullableInt32(reader, 5);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 6);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeOfficeAddress()
  {
    entities.Tier1OfficeAddress.Populated = false;
    entities.Tier1Office.Populated = false;

    return Read("ReadOfficeOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.OutDocRtrnAddr.OfficeSysGenId);
      },
      (db, reader) =>
      {
        entities.Tier1Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Tier1OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Tier1Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Tier1Office.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.Tier1Office.Name = db.GetString(reader, 3);
        entities.Tier1Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 4);
        entities.Tier1Office.MainFaxAreaCode = db.GetNullableInt32(reader, 5);
        entities.Tier1Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Tier1OfficeAddress.Type1 = db.GetString(reader, 7);
        entities.Tier1OfficeAddress.Street1 = db.GetString(reader, 8);
        entities.Tier1OfficeAddress.Street2 = db.GetNullableString(reader, 9);
        entities.Tier1OfficeAddress.City = db.GetString(reader, 10);
        entities.Tier1OfficeAddress.StateProvince = db.GetString(reader, 11);
        entities.Tier1OfficeAddress.PostalCode =
          db.GetNullableString(reader, 12);
        entities.Tier1OfficeAddress.Zip = db.GetNullableString(reader, 13);
        entities.Tier1OfficeAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.Tier1OfficeAddress.Country = db.GetString(reader, 15);
        entities.Tier1OfficeAddress.CreatedBy = db.GetString(reader, 16);
        entities.Tier1OfficeAddress.CreatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.Tier1OfficeAddress.LastUpdatedBy =
          db.GetNullableString(reader, 18);
        entities.Tier1OfficeAddress.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 19);
        entities.Tier1OfficeAddress.Zip3 = db.GetNullableString(reader, 20);
        entities.Tier1OfficeAddress.Populated = true;
        entities.Tier1Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 11);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 12);
        entities.Office.TypeCode = db.GetString(reader, 13);
        entities.Office.Name = db.GetString(reader, 14);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 15);
        entities.Office.CogCode = db.GetNullableString(reader, 16);
        entities.Office.EffectiveDate = db.GetDate(reader, 17);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 18);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 20);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 21);
        entities.ServiceProvider.UserId = db.GetString(reader, 22);
        entities.ServiceProvider.LastName = db.GetString(reader, 23);
        entities.ServiceProvider.FirstName = db.GetString(reader, 24);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 25);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 26);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.PostalCode =
          db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.ServiceProviderAddress.Country = db.GetString(reader, 9);
        entities.ServiceProviderAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.ServiceProviderAddress.Populated = true;
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

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Infrastructure infrastructure;
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

    /// <summary>
    /// A value of CurrentPlus1.
    /// </summary>
    [JsonPropertyName("currentPlus1")]
    public DateWorkArea CurrentPlus1
    {
      get => currentPlus1 ??= new();
      set => currentPlus1 = value;
    }

    /// <summary>
    /// A value of Tier1ServiceProvider.
    /// </summary>
    [JsonPropertyName("tier1ServiceProvider")]
    public Common Tier1ServiceProvider
    {
      get => tier1ServiceProvider ??= new();
      set => tier1ServiceProvider = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public OutDocRtrnAddr Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
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

    private DateWorkArea currentPlus1;
    private Common tier1ServiceProvider;
    private CodeValue codeValue;
    private ServiceProvider serviceProvider;
    private Office office;
    private BatchConvertNumToText batchConvertNumToText;
    private Field field;
    private DateWorkArea max;
    private OutDocRtrnAddr alternate;
    private Document document;
    private DateWorkArea null1;
    private DateWorkArea zdelLocalCurrent;
    private OutDocRtrnAddr outDocRtrnAddr;
    private Array<AddressGroup> address;
    private Field previous;
    private FieldValue fieldValue;
    private DateWorkArea dateWorkArea;
    private SpPrintWorkSet spPrintWorkSet;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private Common processGroup;
    private Field temp;
    private Common position;
    private Field current;
    private Field currentRoot;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ClientOffice.
    /// </summary>
    [JsonPropertyName("clientOffice")]
    public Office ClientOffice
    {
      get => clientOffice ??= new();
      set => clientOffice = value;
    }

    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of GardenCity.
    /// </summary>
    [JsonPropertyName("gardenCity")]
    public Office GardenCity
    {
      get => gardenCity ??= new();
      set => gardenCity = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Tier1OfficeAddress.
    /// </summary>
    [JsonPropertyName("tier1OfficeAddress")]
    public OfficeAddress Tier1OfficeAddress
    {
      get => tier1OfficeAddress ??= new();
      set => tier1OfficeAddress = value;
    }

    /// <summary>
    /// A value of Tier1Office.
    /// </summary>
    [JsonPropertyName("tier1Office")]
    public Office Tier1Office
    {
      get => tier1Office ??= new();
      set => tier1Office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    private Office clientOffice;
    private Office customerServiceCenter;
    private Office gardenCity;
    private CodeValue codeValue;
    private Code code;
    private OfficeAddress tier1OfficeAddress;
    private Office tier1Office;
    private OfficeAddress officeAddress;
    private ServiceProviderAddress serviceProviderAddress;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private Office office;
    private CseOrganization cseOrganization;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private OutDocRtrnAddr outDocRtrnAddr;
  }
#endregion
}
