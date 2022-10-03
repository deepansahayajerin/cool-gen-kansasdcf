// Program: FN_B694_CAB_PROCESS_ADDR_CHANGES, ID: 373014781, model: 746.
// Short name: SWE02954
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B694_CAB_PROCESS_ADDR_CHANGES.
/// </summary>
[Serializable]
public partial class FnB694CabProcessAddrChanges: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B694_CAB_PROCESS_ADDR_CHANGES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB694CabProcessAddrChanges(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB694CabProcessAddrChanges.
  /// </summary>
  public FnB694CabProcessAddrChanges(IContext context, Import import,
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
    // ************************************************
    // *         M A I N T E N A N C E   L O G        *
    // *
    // * Date      Developer	        Description
    // *----------------------------------------------------
    //  08-16-01  Mark Ashworth 	Initial Development
    //  03-18-02  Mark Ashworth	PR 140495 added code to change action code to 
    // EADD
    //                                 
    // if address is end dated.
    //  12-13-10  J Huss		CQ# 9690.  Added Updated By field to address record.
    // 				Set FVI to spaces as it is now sent as part of the
    // 				FVI extract process (SWEFB603/SRRUN248).
    // ************************************************
    local.Escape.Flag = "Y";

    foreach(var item in ReadCsePersonCsePersonAddress())
    {
      if (Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        continue;
      }
      else
      {
        local.Previous.Number = entities.CsePerson.Number;
      }

      local.FoundPerson.Flag = "N";
      ++export.AddressesRead.Count;

      if (AsChar(local.Escape.Flag) == 'Y')
      {
        // ***********************************************************
        // Find out if person is NCP
        // ************************************************************
        if (ReadLegalActionPerson())
        {
          // ***********************************************************
          // Go Write records
          // ************************************************************
          local.FoundPerson.Flag = "Y";

          goto Test;
        }

        // ***********************************************************
        // Find out if person is CP
        // ************************************************************
        if (ReadLegalActionLegalActionPerson())
        {
          // ***********************************************************
          // Go Write records
          // ************************************************************
          local.FoundPerson.Flag = "Y";

          goto Test;
        }

        // ***********************************************************
        // Find out if person is Applicant Recipient
        // ************************************************************
        // ***********************************************************
        // If the person is not found to be an obligor or a custodial parent, 
        // check if they are a cp that was added after the legal action was
        // created. (i.e. kids were in foster care then mother receives custody
        // ).  In this situation there must be an absent parent and a child on
        // the same legal action for a particular case.
        // ************************************************************
        if (ReadCaseRoleCase())
        {
          if (ReadCaseRoleLegalAction())
          {
            if (ReadCaseRole())
            {
              // ***********************************************************
              // Go Write records
              // ************************************************************
              local.FoundPerson.Flag = "Y";
            }
          }
        }
      }

Test:

      // ***********************************************************
      // Write records
      // ************************************************************
      if (AsChar(local.FoundPerson.Flag) == 'Y')
      {
        // ************************************************
        // *Call EAB to retrieve information about a CSE  *
        // *PERSON from the ADABAS system.                *
        // ************************************************
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPersonBatch();

        if (Equal(local.AbendData.AdabasResponseCd, "0148"))
        {
          UseFnB694PrintErrorLine();
          ExitState = "ADABAS_UNAVAILABLE_RB";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseFnB694PrintErrorLine();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // ********************************************************************
        // Write Header Record to Output File
        // ********************************************************************
        // ********************************************************************
        // PR 140495 added code to change action code to EADD if Address is end 
        // dated. MCA 3-18-02
        // ********************************************************************
        if (Equal(entities.CsePersonAddress.EndDate, new DateTime(2099, 12, 31)))
          
        {
          local.HeaderRecord.ActionCode = "UADD";
        }
        else
        {
          local.HeaderRecord.ActionCode = "EADD";
        }

        local.Send.Parm1 = "GR";
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + local.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Set Data for Participant Record to Output File
        // ******************************************************
        local.ParticipantRecord.RecordType = "5";
        local.ParticipantRecord.Type1 = "I";
        local.ParticipantRecord.FirstName = local.CsePersonsWorkSet.FirstName;
        local.ParticipantRecord.MiddleInitial =
          local.CsePersonsWorkSet.MiddleInitial;
        local.ParticipantRecord.LastName = local.CsePersonsWorkSet.LastName;
        local.ParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
        local.ParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
        local.ParticipantRecord.SrsPersonNumber = entities.CsePerson.Number;
        local.ParticipantRecord.FamilyViolenceIndicator = "";
        local.ParticipantRecord.DateOfBirth =
          NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
        local.ParticipantRecord.Pin = "00000000";
        local.ParticipantRecord.Source = "SRS";

        // ******************************************************
        // Write Participant Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(local.ParticipantRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            local.ParticipantRecord.RecordType + local
            .ParticipantRecord.Role + local.ParticipantRecord.Type1 + local
            .ParticipantRecord.Ssn + local.ParticipantRecord.LastName + local
            .ParticipantRecord.FirstName + local
            .ParticipantRecord.MiddleInitial + local
            .ParticipantRecord.Suffix + local.ParticipantRecord.Gender + local
            .ParticipantRecord.DateOfBirth + local
            .ParticipantRecord.SrsPersonNumber + local
            .ParticipantRecord.FamilyViolenceIndicator + local
            .ParticipantRecord.Pin + local.ParticipantRecord.Source + local
            .ParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ******************************************************
        // Set Data for Address Record to Output File
        // ******************************************************
        local.AddressRecord.RecordType = "6";
        local.AddressRecord.Type1 = "M";
        local.AddressRecord.Source = "SRS";
        local.AddressRecord.City = entities.CsePersonAddress.City ?? Spaces(20);
        local.AddressRecord.Street = entities.CsePersonAddress.Street1 ?? Spaces
          (30);
        local.AddressRecord.Street2 = entities.CsePersonAddress.Street2 ?? Spaces
          (30);
        local.AddressRecord.UpdatedBy =
          entities.CsePersonAddress.LastUpdatedBy ?? Spaces(8);

        if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
        {
          local.AddressRecord.Province = "";
          local.AddressRecord.Country = "";
          local.AddressRecord.State = entities.CsePersonAddress.State ?? Spaces
            (2);

          if (IsEmpty(entities.CsePersonAddress.Zip4))
          {
            local.AddressRecord.PostalCode =
              entities.CsePersonAddress.ZipCode ?? Spaces(10);
          }
          else
          {
            local.AddressRecord.PostalCode =
              entities.CsePersonAddress.ZipCode + "-" + entities
              .CsePersonAddress.Zip4;
          }
        }
        else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
        {
          local.AddressRecord.Province = entities.CsePersonAddress.Province ?? Spaces
            (5);
          local.AddressRecord.Country = entities.CsePersonAddress.Country ?? Spaces
            (20);
          local.AddressRecord.State = "";
          local.AddressRecord.PostalCode =
            entities.CsePersonAddress.PostalCode ?? Spaces(10);
        }

        // ******************************************************
        // Write Address Records to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(local.AddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            local.AddressRecord.RecordType + local.AddressRecord.Street + local
            .AddressRecord.Street2 + local.AddressRecord.City + local
            .AddressRecord.State + local.AddressRecord.PostalCode + local
            .AddressRecord.Country + local.AddressRecord.PhoneNumber + local
            .AddressRecord.Province + local.AddressRecord.Source + local
            .AddressRecord.Type1 + local.AddressRecord.UpdatedBy + local
            .AddressRecord.Filler;
          UseFnExtWriteInterfaceFile();
          ++export.TotalWritten.Count;

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
      }
    }
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseFnB694PrintErrorLine()
  {
    var useImport = new FnB694PrintErrorLine.Import();
    var useExport = new FnB694PrintErrorLine.Export();

    useImport.CsePerson.Number = local.Previous.Number;

    Call(FnB694PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    useImport.FileCount.Count = import.FileCount.Count;
    MoveKpcExternalParms(local.Send, useImport.KpcExternalParms);
    useImport.PrintFileRecord.CourtOrderLine =
      local.PrintFileRecord.CourtOrderLine;
    MoveKpcExternalParms(local.Return1, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.Return1);
  }

  private bool ReadCaseRole()
  {
    entities.Child.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.EndDate = db.GetNullableDate(reader, 4);
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCaseRoleLegalAction()
  {
    entities.AbsentParent.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadCaseRoleLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", import.LastRun.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "kpcDate1", import.LowDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "kpcDate2", import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 9);
        entities.AbsentParent.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePersonAddress()
  {
    entities.CsePerson.Populated = false;
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.LastRun.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", import.LowDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 19);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 20);
        entities.CsePerson.Populated = true;
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private bool ReadLegalActionLegalActionPerson()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "kpcDate1", import.ProcessDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "kpcDate2", import.LowDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 5);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.Role = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "kpcDate1", import.LowDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "kpcDate2", import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalActionPerson.Populated = true;
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
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    private DateWorkArea lowDate;
    private DateWorkArea processDate;
    private DateWorkArea lastRun;
    private HeaderRecord headerRecord;
    private Common fileCount;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressesRead.
    /// </summary>
    [JsonPropertyName("addressesRead")]
    public Common AddressesRead
    {
      get => addressesRead ??= new();
      set => addressesRead = value;
    }

    /// <summary>
    /// A value of TotalWritten.
    /// </summary>
    [JsonPropertyName("totalWritten")]
    public Common TotalWritten
    {
      get => totalWritten ??= new();
      set => totalWritten = value;
    }

    private Common addressesRead;
    private Common totalWritten;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Escape.
    /// </summary>
    [JsonPropertyName("escape")]
    public Common Escape
    {
      get => escape ??= new();
      set => escape = value;
    }

    /// <summary>
    /// A value of FoundPerson.
    /// </summary>
    [JsonPropertyName("foundPerson")]
    public Common FoundPerson
    {
      get => foundPerson ??= new();
      set => foundPerson = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public KpcExternalParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public KpcExternalParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of ParticipantRecord.
    /// </summary>
    [JsonPropertyName("participantRecord")]
    public ParticipantRecord ParticipantRecord
    {
      get => participantRecord ??= new();
      set => participantRecord = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of AddressRecord.
    /// </summary>
    [JsonPropertyName("addressRecord")]
    public AddressRecord AddressRecord
    {
      get => addressRecord ??= new();
      set => addressRecord = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    private CsePerson previous;
    private Common escape;
    private Common foundPerson;
    private KpcExternalParms send;
    private PrintFileRecord printFileRecord;
    private KpcExternalParms return1;
    private ParticipantRecord participantRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common ae;
    private Common closeInd;
    private AddressRecord addressRecord;
    private HeaderRecord headerRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private Case1 case1;
    private CaseRole child;
    private CaseRole absentParent;
    private CsePerson csePerson;
    private CaseRole applicantRecipient;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CsePersonAddress csePersonAddress;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private CsePersonAccount obligor;
  }
#endregion
}
