// Program: SI_IIFI_READ_INCOMING_FOR_INFO, ID: 372500275, model: 746.
// Short name: SWE02449
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
/// A program: SI_IIFI_READ_INCOMING_FOR_INFO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiIifiReadIncomingForInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIFI_READ_INCOMING_FOR_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIifiReadIncomingForInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIifiReadIncomingForInfo.
  /// </summary>
  public SiIifiReadIncomingForInfo(IContext context, Import import,
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
    // 02/22/99  C Scroggins     Initial Development
    // ----------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    UseOeCabSetMnemonics();
    MoveInterstateRequest(import.InterstateRequest, export.InterstateRequest);
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.RetcompInd.Flag = import.RetcompInd.Flag;
    export.OtherState.Assign(import.OtherState);

    if (ReadCase())
    {
      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(export.Case1.Status) == 'C')
    {
      export.InterstateRequestCount.Count = 0;

      foreach(var item in ReadInterstateRequest5())
      {
        ++export.InterstateRequestCount.Count;

        if (!IsEmpty(entities.InterstateRequest.Country))
        {
          if (!IsEmpty(import.InterstateRequest.Country))
          {
            if (Equal(entities.InterstateRequest.Country,
              import.InterstateRequest.Country))
            {
              export.InterstateRequest.Assign(entities.InterstateRequest);
            }
          }
          else
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);
          }
        }
      }
    }

    // ----------------------------------------------------------------------------
    // If the case contains multiple AP's, flow to the Case_Composition screen.
    // ----------------------------------------------------------------------------
    if (IsEmpty(import.Ap.Number))
    {
      foreach(var item in ReadCsePersonAbsentParent())
      {
        export.Ap.Number = entities.Ap.Number;
        ++local.Common.Count;

        if (local.Common.Count > 1)
        {
          if (AsChar(export.RetcompInd.Flag) == 'N')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
        }
      }
    }

    if (ReadApplicantRecipientCsePerson())
    {
      export.Ar.Number = entities.Ar.Number;
      UseSiReadCsePerson1();
    }

    if (ReadAbsentParentCsePerson())
    {
      export.Ap.Number = entities.Ap.Number;
      UseSiReadCsePerson2();
    }
    else
    {
      ExitState = "NO_APS_ON_A_CASE";
    }

    if (IsEmpty(export.InterstateRequest.Country))
    {
      local.ForeignCount.Count = 0;
      local.StateCount.Count = 0;

      foreach(var item in ReadInterstateRequest4())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          return;
        }

        if (!IsEmpty(entities.InterstateRequest.Country))
        {
          ++local.ForeignCount.Count;
          export.InterstateRequest.Country = entities.InterstateRequest.Country;
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;
        }

        if (entities.InterstateRequest.OtherStateFips != 0)
        {
          ++local.StateCount.Count;
        }
      }

      if (local.ForeignCount.Count > 0)
      {
        export.ForeignInd.Flag = "Y";
      }
      else
      {
        export.ForeignInd.Flag = "N";
      }

      if (local.StateCount.Count > 0)
      {
        export.StateInd.Flag = "Y";
      }
      else
      {
        export.StateInd.Flag = "N";
      }

      if (AsChar(export.Case1.Status) == 'O')
      {
        if (local.ForeignCount.Count == 0)
        {
          if (local.StateCount.Count == 0)
          {
            ExitState = "CASE_NOT_INTERSTATE";
          }
          else
          {
            export.StateInd.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }

          return;
        }

        if (local.ForeignCount.Count > 1)
        {
          export.ForeignInd.Flag = "Y";
          export.InterstateRequest.Country = "";

          if (local.StateCount.Count > 0)
          {
            export.StateInd.Flag = "Y";
          }

          ExitState = "SI0000_MULTIPLE_IR_EXIST_ON_IREQ";

          return;
        }

        if (local.ForeignCount.Count == 1)
        {
          export.ForeignInd.Flag = "Y";

          if (local.StateCount.Count > 0)
          {
            export.StateInd.Flag = "Y";
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }
      else
      {
        return;
      }

      if (ReadInterstateRequest2())
      {
        export.InterstateRequest.Assign(entities.InterstateRequest);
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else if (AsChar(export.Case1.Status) == 'O')
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }
    else
    {
    }

    if (ReadInterstateRequest1())
    {
      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
      {
        ExitState = "CASE_NOT_IC_INTERSTATE";

        return;
      }

      export.InterstateRequest.Assign(entities.InterstateRequest);
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
    else if (ReadInterstateRequest3())
    {
      if (AsChar(export.Case1.Status) == 'O')
      {
        ExitState = "AP_NOT_IN_FOR_INTERSTATE";

        return;
      }
    }
    else if (AsChar(export.Case1.Status) == 'O')
    {
      ExitState = "CASE_NOT_INTERSTATE";

      return;
    }

    export.CaseClosureDesc.Cdvalue =
      entities.InterstateRequest.OtherStateCaseClosureReason ?? Spaces(10);
    UseCabGetCodeValueDescription();

    if (ReadInterstateRequestAttachment())
    {
      export.AttachmentRcvd.SelectChar = "Y";
    }
    else
    {
      export.AttachmentRcvd.SelectChar = "N";
    }

    if (ReadInterstateContact())
    {
      local.Contact.FirstName = entities.InterstateContact.NameFirst ?? Spaces
        (12);
      local.Contact.LastName = entities.InterstateContact.NameLast ?? Spaces
        (17);
      local.Contact.MiddleInitial = entities.InterstateContact.NameMiddle ?? Spaces
        (1);
      UseSiFormatCsePersonName();
      export.InterstateContact2.FormattedNameText = local.Contact.FormattedName;
      export.InterstateContact1.Assign(entities.InterstateContact);

      if (ReadInterstateContactAddress1())
      {
        if (!IsEmpty(entities.InterstateContactAddress.Country))
        {
          export.InterstateContactAddress.Assign(
            entities.InterstateContactAddress);
        }
      }

      foreach(var item in ReadInterstateContactAddress2())
      {
        if (!IsEmpty(entities.InterstateContactAddress.Country))
        {
          export.InterstateContactAddress.Assign(
            entities.InterstateContactAddress);
        }
      }
    }

    if (ReadInterstatePaymentAddress())
    {
      if (!IsEmpty(entities.InterstatePaymentAddress.Country))
      {
        export.InterstatePaymentAddress.
          Assign(entities.InterstatePaymentAddress);
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.CsenetCaseClosure.CodeName;
    useImport.CodeValue.Cdvalue = export.CaseClosureDesc.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, export.CaseClosureDesc);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.CsenetCaseClosure.CodeName = useExport.CsenetCaseClosure.CodeName;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.Contact);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.Contact.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
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

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
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
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadApplicantRecipientCsePerson",
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
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
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
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
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
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.CreatedBy = db.GetString(reader, 4);
        entities.InterstateContact.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 6);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 7);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 12);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 14);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstateContactAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress1",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 3);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 4);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 5);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 6);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 11);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 16);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadInterstateContactAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return ReadEach("ReadInterstateContactAddress2",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 3);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 4);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 5);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 6);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 11);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 16);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);

        return true;
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "addressStartDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.Zip5 =
          db.GetNullableString(reader, 6);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 8);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 9);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 10);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.InterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.Province =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.PostalCode =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Country =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 17);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
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
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", local.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest4()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest4",
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
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestAttachment()
  {
    entities.InterstateRequestAttachment.Populated = false;

    return Read("ReadInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.Populated = true;
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
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
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

    private Common retcompInd;
    private CsePersonsWorkSet ar;
    private Fips otherState;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CsePersonsWorkSet ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    /// <summary>
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
    }

    /// <summary>
    /// A value of StateInd.
    /// </summary>
    [JsonPropertyName("stateInd")]
    public Common StateInd
    {
      get => stateInd ??= new();
      set => stateInd = value;
    }

    /// <summary>
    /// A value of ForeignInd.
    /// </summary>
    [JsonPropertyName("foreignInd")]
    public Common ForeignInd
    {
      get => foreignInd ??= new();
      set => foreignInd = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of InterstateContact1.
    /// </summary>
    [JsonPropertyName("interstateContact1")]
    public InterstateContact InterstateContact1
    {
      get => interstateContact1 ??= new();
      set => interstateContact1 = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of CaseClosureDesc.
    /// </summary>
    [JsonPropertyName("caseClosureDesc")]
    public CodeValue CaseClosureDesc
    {
      get => caseClosureDesc ??= new();
      set => caseClosureDesc = value;
    }

    /// <summary>
    /// A value of AttachmentRcvd.
    /// </summary>
    [JsonPropertyName("attachmentRcvd")]
    public Common AttachmentRcvd
    {
      get => attachmentRcvd ??= new();
      set => attachmentRcvd = value;
    }

    /// <summary>
    /// A value of InterstateContact2.
    /// </summary>
    [JsonPropertyName("interstateContact2")]
    public OeWorkGroup InterstateContact2
    {
      get => interstateContact2 ??= new();
      set => interstateContact2 = value;
    }

    private Common interstateRequestCount;
    private Common retcompInd;
    private Common stateInd;
    private Common foreignInd;
    private Case1 case1;
    private Fips otherState;
    private InterstateContact interstateContact1;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private CodeValue caseClosureDesc;
    private Common attachmentRcvd;
    private OeWorkGroup interstateContact2;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of StateCount.
    /// </summary>
    [JsonPropertyName("stateCount")]
    public Common StateCount
    {
      get => stateCount ??= new();
      set => stateCount = value;
    }

    /// <summary>
    /// A value of ForeignCount.
    /// </summary>
    [JsonPropertyName("foreignCount")]
    public Common ForeignCount
    {
      get => foreignCount ??= new();
      set => foreignCount = value;
    }

    /// <summary>
    /// A value of MultipleIrForAp.
    /// </summary>
    [JsonPropertyName("multipleIrForAp")]
    public Common MultipleIrForAp
    {
      get => multipleIrForAp ??= new();
      set => multipleIrForAp = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public CsePersonsWorkSet Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of CsenetCaseClosure.
    /// </summary>
    [JsonPropertyName("csenetCaseClosure")]
    public Code CsenetCaseClosure
    {
      get => csenetCaseClosure ??= new();
      set => csenetCaseClosure = value;
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

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Code country;
    private Common stateCount;
    private Common foreignCount;
    private Common multipleIrForAp;
    private InterstateRequest interstateRequest;
    private DateWorkArea current;
    private CsePersonsWorkSet contact;
    private Code csenetCaseClosure;
    private Common error;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestAttachment.
    /// </summary>
    [JsonPropertyName("interstateRequestAttachment")]
    public InterstateRequestAttachment InterstateRequestAttachment
    {
      get => interstateRequestAttachment ??= new();
      set => interstateRequestAttachment = value;
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
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Fips State
    {
      get => state ??= new();
      set => state = value;
    }

    private InterstateRequestAttachment interstateRequestAttachment;
    private Fips fips;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson ap;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private Fips state;
  }
#endregion
}
