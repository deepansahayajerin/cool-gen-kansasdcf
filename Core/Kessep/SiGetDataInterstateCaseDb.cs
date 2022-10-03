// Program: SI_GET_DATA_INTERSTATE_CASE_DB, ID: 373010364, model: 746.
// Short name: SWE02735
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_DATA_INTERSTATE_CASE_DB.
/// </summary>
[Serializable]
public partial class SiGetDataInterstateCaseDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_DATA_INTERSTATE_CASE_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetDataInterstateCaseDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetDataInterstateCaseDb.
  /// </summary>
  public SiGetDataInterstateCaseDb(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ----------------------------------------------------------------------------
    // 06/26/2001	M Ramirez			Initial development
    // ----------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // PROCESSING NOTES:
    // export cse_persons_work_set names are only returned if the Case and 
    // Person numbers are not imported
    // ----------------------------------------------------------------------------
    // *************************************************************
    // 07/12/07  G. Pan   PR218020 Added one line of code
    // 		     OR export interstate_case action_code = "U"
    //                    added two imports entities when calling
    //                      si_get_payment_mailing_address
    // *************************************************************
    if (Lt(local.Current.Date, import.Current.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    UseSiGenCsenetTransactSerialNo();
    export.InterstateCase.FunctionalTypeCode =
      import.InterstateCase.FunctionalTypeCode;
    export.InterstateCase.ActionCode = import.InterstateCase.ActionCode;
    export.InterstateCase.ActionReasonCode =
      import.InterstateCase.ActionReasonCode ?? "";
    export.InterstateCase.LocalFipsState = 20;
    export.InterstateCase.LocalFipsCounty = 0;
    export.InterstateCase.LocalFipsLocation = 0;
    export.InterstateCase.OtherFipsState = import.InterstateCase.OtherFipsState;
    export.InterstateCase.OtherFipsCounty =
      import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    export.InterstateCase.OtherFipsLocation =
      import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    export.InterstateCase.InterstateCaseId =
      import.InterstateCase.InterstateCaseId ?? "";
    export.InterstateCase.ActionResolutionDate = local.Current.Date;
    export.InterstateCase.AttachmentsInd = import.InterstateCase.AttachmentsInd;
    export.InterstateCase.CaseDataInd = 0;
    export.InterstateCase.CollectionDataInd = 0;
    export.InterstateCase.InformationInd = 0;
    export.InterstateCase.ApIdentificationInd = 0;
    export.InterstateCase.ApLocateDataInd = 0;
    export.InterstateCase.ParticipantDataInd = 0;
    export.InterstateCase.OrderDataInd = 0;

    // -------------------------------------------------------------------
    // Verify and/or find Case number
    // -------------------------------------------------------------------
    if (!IsEmpty(import.Case1.Number))
    {
      local.Case1.Number = import.Case1.Number;
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else
    {
      if (!IsEmpty(import.FindCaseFromAp.Number))
      {
        export.CsePersonsWorkSet.Number = import.FindCaseFromAp.Number;
      }
      else
      {
        if (!IsEmpty(import.FindCaseFromAp.Ssn))
        {
          UseEabReadCsePersonUsingSsn1();
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (!IsEmpty(import.FindCaseFromApSsns.Ssn))
          {
            local.CsePersonsWorkSet.Ssn = import.FindCaseFromApSsns.Ssn ?? Spaces
              (9);
            UseEabReadCsePersonUsingSsn2();
          }
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (!IsEmpty(import.FindCaseFromApSsns.AliasSsn1))
          {
            local.CsePersonsWorkSet.Ssn =
              import.FindCaseFromApSsns.AliasSsn1 ?? Spaces(9);
            UseEabReadCsePersonUsingSsn2();
          }
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (!IsEmpty(import.FindCaseFromApSsns.AliasSsn2))
          {
            local.CsePersonsWorkSet.Ssn =
              import.FindCaseFromApSsns.AliasSsn2 ?? Spaces(9);
            UseEabReadCsePersonUsingSsn2();
          }
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          ExitState = "CASE_NF";

          return;
        }
      }

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        ExitState = "CASE_NF";

        return;
      }

      if (ReadCaseCaseRole())
      {
        local.Case1.Number = entities.Case1.Number;
      }

      if (IsEmpty(local.Case1.Number))
      {
        ExitState = "CASE_NF";

        return;
      }
    }

    if (ReadCase())
    {
      export.InterstateCase.KsCaseId = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(entities.Case1.Status) == 'C' && !
      Lt(local.Current.Date, entities.Case1.StatusDate))
    {
      UseSiReadArrOnlyCasePrgType();
      export.InterstateCase.CaseType = local.Program.Code;
      export.InterstateCase.CaseStatus = "C";
    }
    else
    {
      UseSiReadCaseProgramType();

      if (IsEmpty(local.Program.Code))
      {
        ExitState = "ACO_NN0000_ALL_OK";
        UseSiReadArrOnlyCasePrgType();
      }

      export.InterstateCase.CaseType = local.Program.Code;
      export.InterstateCase.CaseStatus = "O";
    }

    foreach(var item in ReadCaseAssignment())
    {
      if (ReadServiceProviderOfficeServiceProviderOffice())
      {
        export.InterstateCase.ContactNameFirst =
          entities.ServiceProvider.FirstName;
        export.InterstateCase.ContactNameMiddle =
          entities.ServiceProvider.MiddleInitial;
        export.InterstateCase.ContactNameLast =
          entities.ServiceProvider.LastName;
        export.InterstateCase.ContactInternetAddress =
          entities.ServiceProvider.EmailAddress;

        if (entities.OfficeServiceProvider.WorkPhoneAreaCode > 0 && entities
          .OfficeServiceProvider.WorkPhoneNumber > 0)
        {
          export.InterstateCase.ContactAreaCode =
            entities.OfficeServiceProvider.WorkPhoneAreaCode;
          export.InterstateCase.ContactPhoneNum =
            entities.OfficeServiceProvider.WorkPhoneNumber;
          export.InterstateCase.ContactPhoneExtension =
            entities.OfficeServiceProvider.WorkPhoneExtension;
        }
        else if (Lt(0, entities.Office.MainPhoneAreaCode) && Lt
          (0, entities.Office.MainPhoneNumber))
        {
          export.InterstateCase.ContactAreaCode =
            entities.Office.MainPhoneAreaCode;
          export.InterstateCase.ContactPhoneNum =
            entities.Office.MainPhoneNumber;
          export.InterstateCase.ContactPhoneExtension = "";
        }
        else
        {
          export.InterstateCase.ContactAreaCode = 0;
          export.InterstateCase.ContactPhoneNum = 0;
          export.InterstateCase.ContactPhoneExtension = "";
        }

        if (Lt(0, entities.OfficeServiceProvider.WorkFaxAreaCode) && Lt
          (0, entities.OfficeServiceProvider.WorkFaxNumber))
        {
          export.InterstateCase.ContactFaxAreaCode =
            entities.OfficeServiceProvider.WorkFaxAreaCode;
          export.InterstateCase.ContactFaxNumber =
            entities.OfficeServiceProvider.WorkFaxNumber;
        }
        else if (Lt(0, entities.Office.MainFaxAreaCode) && Lt
          (0, entities.Office.MainFaxPhoneNumber))
        {
          export.InterstateCase.ContactFaxAreaCode =
            entities.Office.MainFaxAreaCode;
          export.InterstateCase.ContactFaxNumber =
            entities.Office.MainFaxPhoneNumber;
        }
        else
        {
          export.InterstateCase.ContactFaxAreaCode = 0;
          export.InterstateCase.ContactFaxNumber = 0;
        }

        if (ReadServiceProviderAddress())
        {
          export.InterstateCase.ContactAddressLine1 =
            entities.ServiceProviderAddress.Street1;
          export.InterstateCase.ContactAddressLine2 =
            entities.ServiceProviderAddress.Street2;
          export.InterstateCase.ContactCity =
            entities.ServiceProviderAddress.City;
          export.InterstateCase.ContactState =
            entities.ServiceProviderAddress.StateProvince;
          export.InterstateCase.ContactZipCode5 =
            entities.ServiceProviderAddress.Zip;
          export.InterstateCase.ContactZipCode4 =
            entities.ServiceProviderAddress.Zip4;
        }
        else if (ReadOfficeAddress())
        {
          export.InterstateCase.ContactAddressLine1 =
            entities.OfficeAddress.Street1;
          export.InterstateCase.ContactAddressLine2 =
            entities.OfficeAddress.Street2;
          export.InterstateCase.ContactCity = entities.OfficeAddress.City;
          export.InterstateCase.ContactState =
            entities.OfficeAddress.StateProvince;
          export.InterstateCase.ContactZipCode5 = entities.OfficeAddress.Zip;
          export.InterstateCase.ContactZipCode4 = entities.OfficeAddress.Zip4;
        }

        break;
      }
    }

    if (AsChar(export.InterstateCase.ActionCode) == 'R' || AsChar
      (export.InterstateCase.ActionCode) == 'U')
    {
      if (Equal(export.InterstateCase.FunctionalTypeCode, "ENF") || Equal
        (export.InterstateCase.FunctionalTypeCode, "EST") || Equal
        (export.InterstateCase.FunctionalTypeCode, "PAT"))
      {
        if (import.LegalActions.Count > 0)
        {
          for(import.LegalActions.Index = 0; import.LegalActions.Index < import
            .LegalActions.Count; ++import.LegalActions.Index)
          {
            if (!import.LegalActions.CheckSize())
            {
              break;
            }

            if (ReadLegalAction2())
            {
              if (!IsEmpty(entities.CourtCase.PaymentLocation))
              {
                local.LegalAction.Identifier = entities.CourtCase.Identifier;

                break;
              }
            }
            else
            {
              continue;
            }

            if (!ReadTribunal())
            {
              continue;
            }

            if (ReadLegalAction1())
            {
              local.LegalAction.Identifier = entities.LegalAction.Identifier;

              break;
            }
          }

          import.LegalActions.CheckIndex();
        }
        else if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (ReadLegalActionCaseRoleLegalActionPersonCsePerson1())
          {
            local.LegalAction.Identifier = entities.LegalAction.Identifier;
          }
        }
        else if (ReadLegalActionCaseRoleLegalActionPersonCsePerson2())
        {
          local.LegalAction.Identifier = entities.LegalAction.Identifier;
        }

        if (local.LegalAction.Identifier > 0)
        {
          UseSiGetPaymentMailingAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        if (IsEmpty(export.InterstateCase.PaymentMailingAddressLine1))
        {
          ExitState = "SI0000_PAYMENT_ADDRESS_NF_RB";

          return;
        }
      }
    }

    export.InterstateCase.CaseDataInd = 1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Case1.Number = useImport.Case1.Number;
  }

  private void UseEabReadCsePersonUsingSsn1()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = import.FindCaseFromAp.Ssn;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabReadCsePersonUsingSsn2()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase1(useExport.InterstateCase, export.InterstateCase);
  }

  private void UseSiGetPaymentMailingAddress()
  {
    var useImport = new SiGetPaymentMailingAddress.Import();
    var useExport = new SiGetPaymentMailingAddress.Export();

    useImport.Ap.Number = entities.CsePerson.Number;
    useImport.Case1.Number = import.Case1.Number;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(SiGetPaymentMailingAddress.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, export.InterstateCase);
  }

  private void UseSiReadArrOnlyCasePrgType()
  {
    var useImport = new SiReadArrOnlyCasePrgType.Import();
    var useExport = new SiReadArrOnlyCasePrgType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadArrOnlyCasePrgType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.CaseRole.CspNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.CourtCase.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.CourtCase.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", import.LegalActions.Item.G.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCase.Identifier = db.GetInt32(reader, 0);
        entities.CourtCase.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.CourtCase.PaymentLocation = db.GetNullableString(reader, 2);
        entities.CourtCase.TrbId = db.GetNullableInt32(reader, 3);
        entities.CourtCase.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRoleLegalActionPersonCsePerson1()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadLegalActionCaseRoleLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.CaseRole.CasNumber = db.GetString(reader, 4);
        entities.CaseRole.CspNumber = db.GetString(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 10);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 11);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 12);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 13);
        entities.LegalActionPerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadLegalActionCaseRoleLegalActionPersonCsePerson2()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadLegalActionCaseRoleLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.CaseRole.CasNumber = db.GetString(reader, 4);
        entities.CaseRole.CspNumber = db.GetString(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 10);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 11);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 12);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 13);
        entities.LegalActionPerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
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
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 8);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 9);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 12);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 13);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 14);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 15);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 16);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 17);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.CourtCase.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.CourtCase.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
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
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalAction G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private LegalAction g;
    }

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of FindCaseFromAp.
    /// </summary>
    [JsonPropertyName("findCaseFromAp")]
    public CsePersonsWorkSet FindCaseFromAp
    {
      get => findCaseFromAp ??= new();
      set => findCaseFromAp = value;
    }

    /// <summary>
    /// A value of FindCaseFromApSsns.
    /// </summary>
    [JsonPropertyName("findCaseFromApSsns")]
    public InterstateApIdentification FindCaseFromApSsns
    {
      get => findCaseFromApSsns ??= new();
      set => findCaseFromApSsns = value;
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
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
    }

    /// <summary>
    /// A value of ZdelImportPaymentAddress.
    /// </summary>
    [JsonPropertyName("zdelImportPaymentAddress")]
    public LegalAction ZdelImportPaymentAddress
    {
      get => zdelImportPaymentAddress ??= new();
      set => zdelImportPaymentAddress = value;
    }

    private InterstateCase interstateCase;
    private Case1 case1;
    private CsePersonsWorkSet findCaseFromAp;
    private InterstateApIdentification findCaseFromApSsns;
    private DateWorkArea current;
    private Array<LegalActionsGroup> legalActions;
    private LegalAction zdelImportPaymentAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private InterstateCase interstateCase;
    private Case1 case1;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CourtCase.
    /// </summary>
    [JsonPropertyName("courtCase")]
    public LegalAction CourtCase
    {
      get => courtCase ??= new();
      set => courtCase = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
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

    private Tribunal tribunal;
    private LegalAction courtCase;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CaseAssignment caseAssignment;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeAddress officeAddress;
  }
#endregion
}
