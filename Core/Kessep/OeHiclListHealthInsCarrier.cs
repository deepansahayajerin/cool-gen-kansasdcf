// Program: OE_HICL_LIST_HEALTH_INS_CARRIER, ID: 371860581, model: 746.
// Short name: SWE00926
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
/// A program: OE_HICL_LIST_HEALTH_INS_CARRIER.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeHiclListHealthInsCarrier: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICL_LIST_HEALTH_INS_CARRIER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiclListHealthInsCarrier(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiclListHealthInsCarrier.
  /// </summary>
  public OeHiclListHealthInsCarrier(IContext context, Import import,
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
    // ---------------------------------------------
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 01/31/95  Sid             Rework and Completion.
    // 01/19/01  Vithal Madhira  PR# 111764 Health Ins. Co. w/o address  not
    //                           displaying on screen. Fixed this problem.
    // 10/02/01  M. Ashworth	  WR# 20125 Add start and end date to Hico. Changed
    //                           view size from 140 to 110
    // 12/10/02  M. Ashworth	  WR# 20125 added check for 'show all' to only show
    // carriers without 0000 in the carrier code
    // ---------------------------------------------
    // ---------------------------------------------
    // The mailing address is the only address
    // needed to be displayed. So we set a local
    // attribute and set the address type to MAILING
    // and put this attribute in the search cond.
    // ---------------------------------------------
    UseOeCabSetMnemonics();

    // ---------------------------------------------
    // USE a flag local_work_records_read to find
    // out if the search criteria combination has
    // already been used for a search and prevent
    // any further read's.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.WorkRecordsRead.Flag = "N";
    local.MoreRecordsExists.Flag = "N";

    // ---------------------------------------------
    // The user entered a value in the Starting
    // Insurance Company Name field.
    // ---------------------------------------------
    if (!IsEmpty(import.StartingHealthInsuranceCompany.InsurancePolicyCarrier))
    {
      // ---------------------------------------------
      // The user did not enter a value in the
      // Starting City field and did not enter a value
      // in the Starting State field.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (IsEmpty(import.StartingHealthInsuranceCompanyAddress.City) && IsEmpty
          (import.StartingHealthInsuranceCompanyAddress.State))
        {
          local.WorkRecordsRead.Flag = "Y";

          // ------------------------------------------------------------------------
          // Vithal Madhira     01/19/2001        PR#111764
          // Split the READ EACH and read the Company and Address seperately. If
          // address not found send the Company info to PRAD to display on
          // screen.
          // ----------------------------------------------------------------------------
          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompany1())
            {
              if (ReadHealthInsuranceCompanyAddress())
              {
                export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                  entities.HealthInsuranceCompany);
                export.Export1.Update.DetailHealthInsuranceCompanyAddress.
                  Assign(entities.HealthInsuranceCompanyAddress);
              }
              else
              {
                // ------------------------------------------------------------------------
                // Vithal Madhira     01/19/2001        PR#111764
                // ----------------------------------------------------------------------------
                export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                  entities.HealthInsuranceCompany);
              }

              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompany2())
            {
              if (ReadHealthInsuranceCompanyAddress())
              {
                export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                  entities.HealthInsuranceCompany);
                export.Export1.Update.DetailHealthInsuranceCompanyAddress.
                  Assign(entities.HealthInsuranceCompanyAddress);
              }
              else
              {
                // ------------------------------------------------------------------------
                // Vithal Madhira     01/19/2001        PR#111764
                // ----------------------------------------------------------------------------
                export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                  entities.HealthInsuranceCompany);
              }

              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompany.InsurancePolicyCarrier =
              entities.HealthInsuranceCompany.InsurancePolicyCarrier;
          }
        }
      }

      // ---------------------------------------------
      // The user entered a value in the Starting City
      // field but did not enter a value in the
      // Starting State field.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.City) && IsEmpty
          (import.StartingHealthInsuranceCompanyAddress.State))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress6())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress8())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompany.InsurancePolicyCarrier =
              entities.HealthInsuranceCompany.InsurancePolicyCarrier;
            export.LastHealthInsuranceCompanyAddress.City =
              import.StartingHealthInsuranceCompanyAddress.City ?? "";
          }
        }
      }

      // ---------------------------------------------
      // The user entered a value in the Starting
      // State field.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.State) && IsEmpty
          (import.StartingHealthInsuranceCompanyAddress.City))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress9())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress10())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompany.InsurancePolicyCarrier =
              entities.HealthInsuranceCompany.InsurancePolicyCarrier;
            export.LastHealthInsuranceCompanyAddress.State =
              import.StartingHealthInsuranceCompanyAddress.State ?? "";
          }
        }
      }

      // ---------------------------------------------
      // The user entered a value in the Starting
      // State field and the city field.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.State) && !
          IsEmpty(import.StartingHealthInsuranceCompanyAddress.City))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress5())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress7())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            export.LastHealthInsuranceCompany.InsurancePolicyCarrier =
              entities.HealthInsuranceCompany.InsurancePolicyCarrier;
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompanyAddress.City =
              import.StartingHealthInsuranceCompanyAddress.City ?? "";
            export.LastHealthInsuranceCompanyAddress.State =
              import.StartingHealthInsuranceCompanyAddress.State ?? "";
          }
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // The user did not enter a Starting Insurance
      // Company Name.
      // ---------------------------------------------
      // ---------------------------------------------
      // The user entered a Starting City but did not
      // enter a Starting State.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.City) && IsEmpty
          (import.StartingHealthInsuranceCompanyAddress.State))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress2())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress4())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompanyAddress.City =
              entities.HealthInsuranceCompanyAddress.City;
          }
        }
      }

      // ---------------------------------------------
      // The user entered a Starting State but not a
      // Starting City.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.State) && IsEmpty
          (import.StartingHealthInsuranceCompanyAddress.City))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress13())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress14())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompanyAddress.State =
              import.StartingHealthInsuranceCompanyAddress.State ?? "";
            export.LastHealthInsuranceCompanyAddress.City =
              entities.HealthInsuranceCompanyAddress.City;
          }
        }
      }

      // ---------------------------------------------
      // The user entered a value in the Starting
      // State field and the city field.
      // ---------------------------------------------
      if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
      {
        if (!IsEmpty(import.StartingHealthInsuranceCompanyAddress.State) && !
          IsEmpty(import.StartingHealthInsuranceCompanyAddress.City))
        {
          local.WorkRecordsRead.Flag = "Y";

          if (AsChar(import.ShowAll.Flag) == 'N')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress1())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress3())
              
            {
              export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
                entities.HealthInsuranceCompanyAddress);
              export.Export1.Update.DetailHealthInsuranceCompany.Assign(
                entities.HealthInsuranceCompany);
              export.Export1.Next();
            }
          }

          if (export.Export1.IsFull)
          {
            local.MoreRecordsExists.Flag = "Y";
            export.LastHealthInsuranceCompanyAddress.City =
              entities.HealthInsuranceCompanyAddress.City;
            export.LastHealthInsuranceCompanyAddress.State =
              import.StartingHealthInsuranceCompanyAddress.State ?? "";
          }
        }
      }
    }

    // ---------------------------------------------
    // If the user does not enter any fields,
    // display all existing insurance carrier records.
    // ---------------------------------------------
    if (AsChar(local.WorkRecordsRead.Flag) != 'Y')
    {
      if (AsChar(import.ShowAll.Flag) == 'N')
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress11())
          
        {
          local.WorkRecordsRead.Flag = "Y";
          export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
            entities.HealthInsuranceCompanyAddress);
          export.Export1.Update.DetailHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Next();
        }
      }
      else
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress12())
          
        {
          local.WorkRecordsRead.Flag = "Y";
          export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
            entities.HealthInsuranceCompanyAddress);
          export.Export1.Update.DetailHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Next();
        }
      }

      if (export.Export1.IsFull)
      {
        local.MoreRecordsExists.Flag = "Y";
        export.LastHealthInsuranceCompany.InsurancePolicyCarrier =
          entities.HealthInsuranceCompany.InsurancePolicyCarrier;
      }
    }

    // ---------------------------------------------
    // Set Expired indicator
    // ---------------------------------------------
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!Lt(local.Current.Date,
        export.Export1.Item.DetailHealthInsuranceCompany.EndDate))
      {
        export.Export1.Update.DetailExpiredFlag.Flag = "Y";
      }
    }

    // ---------------------------------------------
    // Set the appropriate exit state to display the
    // result of the Search.
    // ---------------------------------------------
    if (export.Export1.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
    else if (AsChar(local.MoreRecordsExists.Flag) == 'Y')
    {
      ExitState = "MORE_RECORDS_EXIST";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MailingAddressType.AddressType =
      useExport.MailingAddressType.AddressType;
  }

  private IEnumerable<bool> ReadHealthInsuranceCompany1()
  {
    return ReadEach("ReadHealthInsuranceCompany1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompany2()
  {
    return ReadEach("ReadHealthInsuranceCompany2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private bool ReadHealthInsuranceCompanyAddress()
  {
    entities.HealthInsuranceCompanyAddress.Populated = false;

    return Read("ReadHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "hicIdentifier", entities.HealthInsuranceCompany.Identifier);
          
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCompanyAddress.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress1()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress10()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress10",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress11()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress11",
      (db, command) =>
      {
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress12()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress12",
      (db, command) =>
      {
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress13()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress13",
      (db, command) =>
      {
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress14()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress14",
      (db, command) =>
      {
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress2()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress3()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress4()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress5()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress6()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress7()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress7",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress8()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress8",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "city",
          import.StartingHealthInsuranceCompanyAddress.City ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress9()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress9",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.StartingHealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
        db.SetNullableString(
          command, "state",
          import.StartingHealthInsuranceCompanyAddress.State ?? "");
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 9);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

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
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of StartingHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress StartingHealthInsuranceCompanyAddress
    {
      get => startingHealthInsuranceCompanyAddress ??= new();
      set => startingHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of StartingHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompany")]
    public HealthInsuranceCompany StartingHealthInsuranceCompany
    {
      get => startingHealthInsuranceCompany ??= new();
      set => startingHealthInsuranceCompany = value;
    }

    private Common showAll;
    private HealthInsuranceCompanyAddress startingHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany startingHealthInsuranceCompany;
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
      /// A value of DetailWork.
      /// </summary>
      [JsonPropertyName("detailWork")]
      public ScrollingAttributes DetailWork
      {
        get => detailWork ??= new();
        set => detailWork = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompany")]
      public HealthInsuranceCompany DetailHealthInsuranceCompany
      {
        get => detailHealthInsuranceCompany ??= new();
        set => detailHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompanyAddress.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompanyAddress")]
      public HealthInsuranceCompanyAddress DetailHealthInsuranceCompanyAddress
      {
        get => detailHealthInsuranceCompanyAddress ??= new();
        set => detailHealthInsuranceCompanyAddress = value;
      }

      /// <summary>
      /// A value of DetailExpiredFlag.
      /// </summary>
      [JsonPropertyName("detailExpiredFlag")]
      public Common DetailExpiredFlag
      {
        get => detailExpiredFlag ??= new();
        set => detailExpiredFlag = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private ScrollingAttributes detailWork;
      private HealthInsuranceCompany detailHealthInsuranceCompany;
      private HealthInsuranceCompanyAddress detailHealthInsuranceCompanyAddress;
      private Common detailExpiredFlag;
    }

    /// <summary>
    /// A value of LastHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("lastHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress LastHealthInsuranceCompanyAddress
    {
      get => lastHealthInsuranceCompanyAddress ??= new();
      set => lastHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of LastHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("lastHealthInsuranceCompany")]
    public HealthInsuranceCompany LastHealthInsuranceCompany
    {
      get => lastHealthInsuranceCompany ??= new();
      set => lastHealthInsuranceCompany = value;
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

    private HealthInsuranceCompanyAddress lastHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany lastHealthInsuranceCompany;
    private Array<ExportGroup> export1;
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
    /// A value of MoreRecordsExists.
    /// </summary>
    [JsonPropertyName("moreRecordsExists")]
    public Common MoreRecordsExists
    {
      get => moreRecordsExists ??= new();
      set => moreRecordsExists = value;
    }

    /// <summary>
    /// A value of WorkRecordsRead.
    /// </summary>
    [JsonPropertyName("workRecordsRead")]
    public Common WorkRecordsRead
    {
      get => workRecordsRead ??= new();
      set => workRecordsRead = value;
    }

    /// <summary>
    /// A value of MailingAddressType.
    /// </summary>
    [JsonPropertyName("mailingAddressType")]
    public HealthInsuranceCompanyAddress MailingAddressType
    {
      get => mailingAddressType ??= new();
      set => mailingAddressType = value;
    }

    /// <summary>
    /// A value of SearchValue.
    /// </summary>
    [JsonPropertyName("searchValue")]
    public AaWork SearchValue
    {
      get => searchValue ??= new();
      set => searchValue = value;
    }

    private DateWorkArea current;
    private Common moreRecordsExists;
    private Common workRecordsRead;
    private HealthInsuranceCompanyAddress mailingAddressType;
    private AaWork searchValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
  }
#endregion
}
