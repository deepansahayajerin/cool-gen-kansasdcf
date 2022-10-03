// Program: FN_RETRIEVE_LEGAL_REF_AND_SP2, ID: 945112054, model: 746.
// Short name: SWE03674
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_RETRIEVE_LEGAL_REF_AND_SP2.
/// </para>
/// <para>
/// This cab retrieves the legal referral and service provider for a given case.
/// It checks to see if legal referral case roles exist for a case, and if so,
/// qualifies the read on case role.
/// </para>
/// </summary>
[Serializable]
public partial class FnRetrieveLegalRefAndSp2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RETRIEVE_LEGAL_REF_AND_SP2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRetrieveLegalRefAndSp2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRetrieveLegalRefAndSp2.
  /// </summary>
  public FnRetrieveLegalRefAndSp2(IContext context, Import import, Export export)
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
    // : Check to see if there are any legal referral case roles.
    // If a legal referral case role is found for the case, it is assumed that
    // this entity should be used to determine referrals.  If one is not found,
    // all persons on the case are assumed to be referred to the service
    // provider.  This is done because most of the referrals that currently 
    // exist were converted, and no legal referral case roles were set up for
    // them.
    if (!ReadCase3())
    {
      // <===========Write error message to error report ========>
      local.External.FileInstruction = "WRITE";
      export.ErrorMessage.RptDetail = "Database problem - Case NF";
      export.ErrorMessage.RptDetail = TrimEnd(export.ErrorMessage.RptDetail) + TrimEnd
        (import.IdentifierForErrMsg.RptDetail) + "  ";
      export.ErrorMessage.RptDetail = TrimEnd(export.ErrorMessage.RptDetail) + " Case:" +
        import.Case1.Number + " with AR:" + import.Ar.Number;
      UseFnEabB608WriteError1();

      return;
    }

    if (ReadLegalReferralCaseRole())
    {
      local.LrefCrolesExist.Flag = "Y";
    }
    else
    {
      local.LrefCrolesExist.Flag = "N";
    }

    local.Common.Count = 0;

    // : If legal referral case roles exist, ensure that the current supported 
    // person is set up on a legal referral case role for the current legal
    // referral.
    // : This read structure contains:
    // 1- Read Legal Referral for situation where Legal Referral Case Roles
    //    exist, qualifying Legal Referral reason code1 on 'ENF' or 'CV'.
    // 2- Read Legal Referral without qualifier on Legal Referral reason code1
    //    for situation where Legal Referral Case Roles exist, and the first 
    // read
    //  did not yield a result.
    // 3,4 - Same as above, for situation where Legal Referral Case Roles do not
    //       exist, so qualifiers are not in the reads.
    if (AsChar(local.LrefCrolesExist.Flag) == 'Y')
    {
      // : Legal Referral Case Roles exist, so qualify the read.
      if (!ReadLegalReferralLegalReferralAssignment1())
      {
        // : Read again, without qualifiers on Legal Referral reason code of ENF
        //  and CV.  This is because it is possible to have referrals for people
        //  who are current, and a referral with ENF will not exist.  But we 
        // need
        //  to look for ENF first.
        if (!ReadLegalReferralLegalReferralAssignment2())
        {
          // WE will try to read one last time this time without tieing it to 
          // the court order number on the collection record.
          if (!ReadLegalReferralLegalReferralAssignment5())
          {
            if (ReadLegalReferralLegalReferralAssignment7())
            {
              if (ReadLegalReferral())
              {
                export.ErrorMessage.RptDetail =
                  "No Actv Leg Referral Assignment Fnd:";
              }
              else
              {
                export.ErrorMessage.RptDetail = "Sup person not on referral:";
              }
            }
            else
            {
              export.ErrorMessage.RptDetail = "No Referral found (read1):";
            }

            if (ReadCase3())
            {
              if (AsChar(entities.Case1.Status) == 'O')
              {
                local.Case1.Number = import.Case1.Number;
                local.CsePerson.Number = import.Ar.Number;
              }
              else if (import.Hardcode.HardcodeVoluntary.
                SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier)
              {
                if (ReadCase2())
                {
                  local.Case1.Number = entities.Case1.Number;

                  if (ReadCsePerson())
                  {
                    local.CsePerson.Number = entities.CsePerson.Number;
                  }
                  else
                  {
                    local.CsePerson.Number = import.Ar.Number;
                  }
                }
                else
                {
                  local.Case1.Number = import.Case1.Number;
                  export.Condition.Text8 = "NO REF";
                  export.ErrorMessage.RptDetail =
                    "No open case for supported person";

                  // : OK
                  return;
                }
              }
              else if (import.Hardcode.HardcodeSpArrearsJudgmt.
                SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier || import
                .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier)
              {
                if (ReadCase1())
                {
                  local.Case1.Number = entities.Case1.Number;

                  if (ReadCsePerson())
                  {
                    local.CsePerson.Number = entities.CsePerson.Number;
                  }
                  else
                  {
                    local.CsePerson.Number = import.Ar.Number;
                  }
                }
                else
                {
                  local.Case1.Number = import.Case1.Number;
                  export.Condition.Text8 = "NO REF";
                  export.ErrorMessage.RptDetail =
                    "No open case for supported person";

                  // : OK
                  return;
                }
              }
              else if (ReadCase2())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (import.Hardcode.HardcodeVoluntary.
              SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier)
            {
              if (ReadCase2())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (import.Hardcode.HardcodeSpArrearsJudgmt.
              SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier || import
              .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier)
            {
              if (ReadCase1())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (ReadCase2())
            {
              local.Case1.Number = entities.Case1.Number;

              if (ReadCsePerson())
              {
                local.CsePerson.Number = entities.CsePerson.Number;
              }
              else
              {
                local.CsePerson.Number = import.Ar.Number;
              }
            }
            else
            {
              local.Case1.Number = import.Case1.Number;
              export.Condition.Text8 = "NO REF";
              export.ErrorMessage.RptDetail =
                "No open case for supported person";

              // : OK
              return;
            }

            if (ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1())
            {
              MoveOffice(entities.Office, local.Office);
              local.ServiceProvider.Assign(entities.ServiceProvider);
            }
            else if (ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2())
              
            {
              MoveOffice(entities.Office, local.Office);
              local.ServiceProvider.Assign(entities.ServiceProvider);
            }

            if (ReadTribunalFips())
            {
              local.Region.Name = entities.Fips.CountyDescription ?? Spaces(20);
            }

            // <===========Temp Message ========>
            if (IsEmpty(local.CsePerson.Number))
            {
              local.CsePerson.Number = import.Ar.Number;
            }

            local.External.FileInstruction = "WRITE";
            export.ErrorMessage.RptDetail =
              TrimEnd(export.ErrorMessage.RptDetail) + TrimEnd
              (import.IdentifierForErrMsg.RptDetail) + "  ";
            export.ErrorMessage.RptDetail =
              TrimEnd(export.ErrorMessage.RptDetail) + " Case:" + local
              .Case1.Number + " with AR:" + local.CsePerson.Number + "  Supported Per: " +
              import.PersistentSupported.Number;
            local.ServiceProvider.SystemGeneratedId =
              entities.ServiceProvider.SystemGeneratedId;
            UseFnEabB608WriteError3();
            export.Condition.Text8 = "NO REF";

            // : OK
            return;
          }
        }
      }
    }
    else
    {
      // : No Legal Referral Case Roles - so this read is not qualified for 
      // that.
      if (!ReadLegalReferralLegalReferralAssignment3())
      {
        // : Read again, without qualifiers on Legal Referral reason code of ENF
        //  and CV.  This is because it is possible to have referrals for people
        //  who are current, and a referral with ENF will not exist.  But we 
        // need
        //  to look for ENF first.
        if (!ReadLegalReferralLegalReferralAssignment4())
        {
          if (!ReadLegalReferralLegalReferralAssignment6())
          {
            if (ReadLegalReferralLegalReferralAssignment7())
            {
              export.ErrorMessage.RptDetail =
                "No Actv Leg Referral Assignment Fnd:";
            }
            else
            {
              export.ErrorMessage.RptDetail = "No Referral Found:";
            }

            if (ReadCase3())
            {
              if (AsChar(entities.Case1.Status) == 'O')
              {
                local.Case1.Number = import.Case1.Number;
                local.CsePerson.Number = import.Ar.Number;
              }
              else if (import.Hardcode.HardcodeVoluntary.
                SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier)
              {
                if (ReadCase2())
                {
                  local.Case1.Number = entities.Case1.Number;

                  if (ReadCsePerson())
                  {
                    local.CsePerson.Number = entities.CsePerson.Number;
                  }
                  else
                  {
                    local.CsePerson.Number = import.Ar.Number;
                  }
                }
                else
                {
                  local.Case1.Number = import.Case1.Number;
                  export.Condition.Text8 = "NO REF";
                  export.ErrorMessage.RptDetail =
                    "No open case for supported person";

                  // : OK
                  return;
                }
              }
              else if (import.Hardcode.HardcodeSpArrearsJudgmt.
                SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier || import
                .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier == import
                .ObligationType.SystemGeneratedIdentifier)
              {
                if (ReadCase1())
                {
                  local.Case1.Number = entities.Case1.Number;

                  if (ReadCsePerson())
                  {
                    local.CsePerson.Number = entities.CsePerson.Number;
                  }
                  else
                  {
                    local.CsePerson.Number = import.Ar.Number;
                  }
                }
                else
                {
                  local.Case1.Number = import.Case1.Number;
                  export.Condition.Text8 = "NO REF";
                  export.ErrorMessage.RptDetail =
                    "No open case for supported person";

                  // : OK
                  return;
                }
              }
              else if (ReadCase2())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (import.Hardcode.HardcodeVoluntary.
              SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier)
            {
              if (ReadCase2())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (import.Hardcode.HardcodeSpArrearsJudgmt.
              SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier || import
              .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier == import
              .ObligationType.SystemGeneratedIdentifier)
            {
              if (ReadCase1())
              {
                local.Case1.Number = entities.Case1.Number;

                if (ReadCsePerson())
                {
                  local.CsePerson.Number = entities.CsePerson.Number;
                }
                else
                {
                  local.CsePerson.Number = import.Ar.Number;
                }
              }
              else
              {
                local.Case1.Number = import.Case1.Number;
                export.Condition.Text8 = "NO REF";
                export.ErrorMessage.RptDetail =
                  "No open case for supported person";

                // : OK
                return;
              }
            }
            else if (ReadCase2())
            {
              local.Case1.Number = entities.Case1.Number;

              if (ReadCsePerson())
              {
                local.CsePerson.Number = entities.CsePerson.Number;
              }
              else
              {
                local.CsePerson.Number = import.Ar.Number;
              }
            }
            else
            {
              local.Case1.Number = import.Case1.Number;
              export.Condition.Text8 = "NO REF";
              export.ErrorMessage.RptDetail =
                "No open case for supported person";

              // : OK
              return;
            }

            if (ReadTribunalFips())
            {
              local.Region.Name = entities.Fips.CountyDescription ?? Spaces(20);
            }

            if (ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1())
            {
              MoveOffice(entities.Office, local.Office);
              local.ServiceProvider.Assign(entities.ServiceProvider);
            }
            else if (ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2())
              
            {
              MoveOffice(entities.Office, local.Office);
              local.ServiceProvider.Assign(entities.ServiceProvider);
            }

            // <===========Temp Message ========>
            if (IsEmpty(local.CsePerson.Number))
            {
              local.CsePerson.Number = import.Ar.Number;
            }

            local.External.FileInstruction = "WRITE";
            export.ErrorMessage.RptDetail =
              TrimEnd(export.ErrorMessage.RptDetail) + TrimEnd
              (import.IdentifierForErrMsg.RptDetail) + "  ";
            export.ErrorMessage.RptDetail =
              TrimEnd(export.ErrorMessage.RptDetail) + " Case:" + local
              .Case1.Number + " with AR:" + local.CsePerson.Number + "  Supported Per: " +
              import.PersistentSupported.Number;
            UseFnEabB608WriteError3();
            export.Condition.Text8 = "NO REF";

            // OK
            return;
          }
        }
      }
    }

    // : Found a Referral - now read the Service Provider.
    if (ReadServiceProviderOfficeServiceProvider())
    {
      if (ReadOffice())
      {
        MoveOffice(entities.Office, export.Office);

        if (ReadCseOrganization())
        {
          MoveCseOrganization(entities.Parent, export.CseOrganization);
        }
      }

      if (Lt(entities.OfficeServiceProvider.DiscontinueDate,
        import.ReadSearchDate.Date) || Lt
        (import.ReadSearchDate.Date,
        entities.OfficeServiceProvider.EffectiveDate))
      {
        // <===========Temp Message ========>
        local.External.FileInstruction = "WRITE";
        export.ErrorMessage.RptDetail = "OSP not active :";
        export.ErrorMessage.RptDetail =
          TrimEnd(export.ErrorMessage.RptDetail) + TrimEnd
          (import.IdentifierForErrMsg.RptDetail) + "  ";
        export.ErrorMessage.RptDetail =
          TrimEnd(export.ErrorMessage.RptDetail) + " Case:" + import
          .Case1.Number + " with AR:" + import.Ar.Number + "  Srvc Prvdr: " + entities
          .ServiceProvider.UserId;
        local.ServiceProvider.SystemGeneratedId =
          entities.ServiceProvider.SystemGeneratedId;
        UseFnEabB608WriteError2();
        export.Condition.Text8 = "OSP EXP";
      }
      else
      {
        export.ServiceProvider.Assign(entities.ServiceProvider);
        export.LegalReferralAssignment.EffectiveDate =
          entities.LegalReferralAssignment.EffectiveDate;
      }
    }
    else
    {
      // <===========Write error message to error report ========>
      local.External.FileInstruction = "WRITE";
      export.ErrorMessage.RptDetail = "Database problem - OSP NF";
      export.ErrorMessage.RptDetail = TrimEnd(export.ErrorMessage.RptDetail) + TrimEnd
        (import.IdentifierForErrMsg.RptDetail) + "  ";
      export.ErrorMessage.RptDetail = TrimEnd(export.ErrorMessage.RptDetail) + " Case:" +
        import.Case1.Number + " with AR:" + import.Ar.Number;
      UseFnEabB608WriteError2();
      export.Condition.Text8 = "OSP NF";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseFnEabB608WriteError1()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Region.Name = local.Region.Name;
    MoveOffice(local.Office, useImport.Office);
    useImport.ServiceProvider.Assign(local.ServiceProvider);
    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.EabReportSend.RptDetail = export.ErrorMessage.RptDetail;
    useExport.External.Assign(export.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    export.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError2()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.EabReportSend.RptDetail = export.ErrorMessage.RptDetail;
    useImport.ServiceProvider.Assign(export.ServiceProvider);
    MoveOffice(export.Office, useImport.Office);
    useImport.Region.Name = export.CseOrganization.Name;
    useExport.External.Assign(export.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    export.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError3()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.Region.Name = local.Region.Name;
    MoveOffice(local.Office, useImport.Office);
    useImport.ServiceProvider.Assign(local.ServiceProvider);
    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.EabReportSend.RptDetail = export.ErrorMessage.RptDetail;
    useExport.External.Assign(export.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    export.External.Assign(useExport.External);
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 9);
        entities.Office.Name = db.GetString(reader, 10);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 11);
        entities.Office.CogCode = db.GetNullableString(reader, 12);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 13);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.ServiceProvider.UserId = db.GetString(reader, 15);
        entities.ServiceProvider.LastName = db.GetString(reader, 16);
        entities.ServiceProvider.FirstName = db.GetString(reader, 17);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 18);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 9);
        entities.Office.Name = db.GetString(reader, 10);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 11);
        entities.Office.CogCode = db.GetNullableString(reader, 12);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 13);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.ServiceProvider.UserId = db.GetString(reader, 15);
        entities.ServiceProvider.LastName = db.GetString(reader, 16);
        entities.ServiceProvider.FirstName = db.GetString(reader, 17);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 18);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.Parent.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.
          SetString(command, "cogParentType", entities.Office.CogTypeCode ?? "");
          
        db.SetString(command, "cogParentCode", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.Parent.Code = db.GetString(reader, 0);
        entities.Parent.Type1 = db.GetString(reader, 1);
        entities.Parent.Name = db.GetString(reader, 2);
        entities.Parent.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.ReadSearchDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferralCaseRole()
  {
    entities.LegalReferralCaseRole.Populated = false;

    return Read("ReadLegalReferralCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumberRole", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralCaseRole.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.LegalReferralCaseRole.CasNumber = db.GetString(reader, 1);
        entities.LegalReferralCaseRole.LgrId = db.GetInt32(reader, 2);
        entities.LegalReferralCaseRole.CasNumberRole = db.GetString(reader, 3);
        entities.LegalReferralCaseRole.CspNumber = db.GetString(reader, 4);
        entities.LegalReferralCaseRole.CroType = db.GetString(reader, 5);
        entities.LegalReferralCaseRole.CroId = db.GetInt32(reader, 6);
        entities.LegalReferralCaseRole.Populated = true;
        CheckValid<LegalReferralCaseRole>("CroType",
          entities.LegalReferralCaseRole.CroType);
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment1()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(
          command, "standardNo", import.Collection.CourtOrderAppliedTo ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment2()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(
          command, "standardNo", import.Collection.CourtOrderAppliedTo ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment3()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(
          command, "standardNo", import.Collection.CourtOrderAppliedTo ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment4()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(
          command, "standardNo", import.Collection.CourtOrderAppliedTo ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment5()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersistentSupported.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment6()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReadSearchDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralLegalReferralAssignment7()
  {
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralLegalReferralAssignment7",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 2);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 2);
        entities.Office.CogCode = db.GetNullableString(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Fips.County = db.GetInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Fips.State = db.GetInt32(reader, 4);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
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
    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
    {
      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      /// <summary>
      /// A value of HardcodeSpousalSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSpousalSupport")]
      public ObligationType HardcodeSpousalSupport
      {
        get => hardcodeSpousalSupport ??= new();
        set => hardcodeSpousalSupport = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgmt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgmt")]
      public ObligationType HardcodeSpArrearsJudgmt
      {
        get => hardcodeSpArrearsJudgmt ??= new();
        set => hardcodeSpArrearsJudgmt = value;
      }

      private ObligationType hardcodeVoluntary;
      private ObligationType hardcodeSpousalSupport;
      private ObligationType hardcodeSpArrearsJudgmt;
    }

    /// <summary>
    /// A value of PersistentSupported.
    /// </summary>
    [JsonPropertyName("persistentSupported")]
    public CsePerson PersistentSupported
    {
      get => persistentSupported ??= new();
      set => persistentSupported = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ReadSearchDate.
    /// </summary>
    [JsonPropertyName("readSearchDate")]
    public DateWorkArea ReadSearchDate
    {
      get => readSearchDate ??= new();
      set => readSearchDate = value;
    }

    /// <summary>
    /// A value of IdentifierForErrMsg.
    /// </summary>
    [JsonPropertyName("identifierForErrMsg")]
    public EabReportSend IdentifierForErrMsg
    {
      get => identifierForErrMsg ??= new();
      set => identifierForErrMsg = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
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

    private CsePerson persistentSupported;
    private CsePerson ar;
    private Case1 case1;
    private DateWorkArea readSearchDate;
    private EabReportSend identifierForErrMsg;
    private Collection collection;
    private LegalAction legalAction;
    private HardcodeGroup hardcode;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Condition.
    /// </summary>
    [JsonPropertyName("condition")]
    public TextWorkArea Condition
    {
      get => condition ??= new();
      set => condition = value;
    }

    /// <summary>
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public EabReportSend ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    private External external;
    private TextWorkArea condition;
    private EabReportSend errorMessage;
    private LegalReferralAssignment legalReferralAssignment;
    private ServiceProvider serviceProvider;
    private Office office;
    private CseOrganization cseOrganization;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Region.
    /// </summary>
    [JsonPropertyName("region")]
    public CseOrganization Region
    {
      get => region ??= new();
      set => region = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of LrefCrolesExist.
    /// </summary>
    [JsonPropertyName("lrefCrolesExist")]
    public Common LrefCrolesExist
    {
      get => lrefCrolesExist ??= new();
      set => lrefCrolesExist = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsePerson csePerson;
    private Case1 case1;
    private CseOrganization region;
    private Office office;
    private ServiceProvider serviceProvider;
    private External external;
    private Common lrefCrolesExist;
    private Common common;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CseOrganization Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public LegalReferral KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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

    private CsePerson csePerson;
    private CaseRole applicantRecipient;
    private Fips fips;
    private Tribunal tribunal;
    private CaseAssignment caseAssignment;
    private CseOrganization child;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization parent;
    private Office office;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private LegalReferral keyOnly;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
