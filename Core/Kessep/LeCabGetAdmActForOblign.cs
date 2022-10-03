// Program: LE_CAB_GET_ADM_ACT_FOR_OBLIGN, ID: 372600224, model: 746.
// Short name: SWE00726
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
/// A program: LE_CAB_GET_ADM_ACT_FOR_OBLIGN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block reads the administrative actions taken for a given 
/// obligation and returns them in a group view.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabGetAdmActForOblign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_GET_ADM_ACT_FOR_OBLIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabGetAdmActForOblign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabGetAdmActForOblign.
  /// </summary>
  public LeCabGetAdmActForOblign(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #
    // Description
    // ????		Initial development
    // 10/01/98     P. Sharp      Phase II assessment changes.
    // Removed reads of cse person, cse person account, and
    // obligation incorporated them as part of the qualifier for the
    // read eaches. Also added enhancement to allow the user to
    // select auto, manual or both admin actions.
    // 082599		RJEAN		PR612-13
    // Change read of Admin Act Cert by Current Amt Date.
    // 010500	RJEAN	PR84255	Bypass any FDSO that have
    // date sent equal zero-date.  Populate FDSO current amount
    // date with date sent so it displays.
    // 04/18/2000	PMcElderry	PR # 79004
    // 09/14/2000	PMcElderry	PR # 102336
    // Code fixes associated to support removal of ADMIN_
    // ACTION_CERT_OBLIGATION and ADM_ACT_CERT_
    // DEBT_DETAIL.
    // *******************************************************************
    export.Export1.Index = -1;
    local.Auto.Index = -1;
    local.SdsoAlreadyMoved.Flag = "N";
    local.FdsoAlreadyMoved.Flag = "N";
    local.CredAlreadyMoved.Flag = "N";
    local.AutoFlag.Flag = "N";
    local.ManualFlag.Flag = "N";

    // ---------------------------------------------------------------
    // Depending on the option to list System generated admin
    // actions OR Manual admin actions use the entity
    // administrative act certification or obligation administrative
    // action. As of 10/1/98 the user now has the option of getting
    // all the admin action. Before they could only select auto or
    // manual. Now if the selection is spaces both type will be read
    // and combined into one group view.
    // ---------------------------------------------------------------
    if (AsChar(import.ListOptManualAutoActs.OneChar) == 'A' || IsEmpty
      (import.ListOptManualAutoActs.OneChar))
    {
      foreach(var item in ReadAdministrativeActCertificationAdministrativeAction())
        
      {
        if (!IsEmpty(import.Required.Type1))
        {
          if (!Equal(entities.AdministrativeAction.Type1, import.Required.Type1))
            
          {
            continue;
          }
        }

        if (Lt(local.InitializedToZeros.Date, import.StartDate.Date))
        {
          if (!Lt(entities.AdministrativeActCertification.TakenDate,
            import.StartDate.Date))
          {
            continue;
          }
        }

        if (Equal(entities.AdministrativeActCertification.Type1, "IRSC"))
        {
          if (Equal(entities.AdministrativeActCertification.
            CseCentralOfficeApprovalDate, null))
          {
            continue;
          }
        }

        if (Equal(entities.AdministrativeActCertification.DateSent,
          local.InitializedToZeros.Date) && Equal
          (entities.AdministrativeActCertification.Type1, "FDSO"))
        {
          continue;
        }

        // ***********************************************************
        // *010500	RJEAN	PR84255	Bypass any FDSO that have date sent
        // * equal zero-date.
        // ***********************************************************
        if (Equal(entities.AdministrativeActCertification.Type1, "FDSO"))
        {
          if (!ReadFederalDebtSetoff())
          {
            ExitState = "FEDERAL_DEBT_SETOFF_NF";

            return;
          }
        }

        if (IsEmpty(import.Required.Type1))
        {
          if (Equal(entities.AdministrativeAction.Type1, "SDSO"))
          {
            if (AsChar(local.SdsoAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.SdsoAlreadyMoved.Flag = "Y";
            }
          }

          if (Equal(entities.AdministrativeAction.Type1, "FDSO"))
          {
            if (AsChar(local.FdsoAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.FdsoAlreadyMoved.Flag = "Y";
            }
          }

          if (Equal(entities.AdministrativeAction.Type1, "CRED"))
          {
            if (AsChar(local.CredAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.CredAlreadyMoved.Flag = "Y";
            }
          }
        }

        if (IsEmpty(import.ListOptManualAutoActs.OneChar))
        {
          ++local.Auto.Index;
          local.Auto.CheckSize();

          local.AutoFlag.Flag = "Y";

          if (local.Auto.Index + 1 >= Local.AutoGroup.Capacity)
          {
            return;
          }
          else
          {
            // ****************************************************************
            // *010500	RJEAN	PR84255	Populate FDSO current
            // amount date with date sent so it displays.
            // ****************************************************************
            if (Equal(entities.AdministrativeActCertification.Type1, "FDSO"))
            {
              local.Auto.Update.AutoAdministrativeActCertification.
                CurrentAmountDate =
                  entities.AdministrativeActCertification.DateSent;
            }

            MoveAdministrativeActCertification1(entities.
              AdministrativeActCertification,
              local.Auto.Update.AutoAdministrativeActCertification);
            local.Auto.Update.AutoAdministrativeAction.Type1 =
              entities.AdministrativeAction.Type1;
          }

          if (Equal(entities.AdministrativeActCertification.Type1, "FDSO"))
          {
            MoveAdministrativeActCertification2(entities.FederalDebtSetoff,
              local.Auto.Update.AutoFederalDebtSetoff);

            if (Equal(entities.AdministrativeActCertification.CurrentAmount,
              entities.FederalDebtSetoff.AdcAmount))
            {
              local.Auto.Update.AutoFederalDebtSetoff.NonAdcAmount = 0;
            }
            else if (Equal(entities.AdministrativeActCertification.
              CurrentAmount, entities.FederalDebtSetoff.NonAdcAmount))
            {
              local.Auto.Update.AutoFederalDebtSetoff.AdcAmount = 0;
            }

            local.Auto.Update.AutoAdministrativeActCertification.CurrentAmount =
              local.Auto.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();
          }

          local.Auto.Update.AutoCommon.SelectChar = "Y";

          if (local.Auto.Index + 1 >= Local.AutoGroup.Capacity)
          {
            return;
          }
        }
        else
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          ++local.Auto.Index;
          local.Auto.CheckSize();

          // ****************************************************************
          // *010500	RJEAN	PR84255	Populate FDSO current
          // amount date with date sent so it displays.
          // ****************************************************************
          if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
          {
            return;
          }
          else
          {
            export.Export1.Update.DetailAdministrativeAction.Type1 =
              entities.AdministrativeAction.Type1;
            local.Auto.Update.AutoAdministrativeAction.Type1 =
              entities.AdministrativeAction.Type1;
            MoveAdministrativeActCertification1(entities.
              AdministrativeActCertification,
              local.Auto.Update.AutoAdministrativeActCertification);
            MoveAdministrativeActCertification1(entities.
              AdministrativeActCertification,
              export.Export1.Update.DetailAdministrativeActCertification);
          }

          if (local.Auto.Index + 1 >= Local.AutoGroup.Capacity)
          {
            return;
          }
          else if (Equal(entities.AdministrativeActCertification.Type1, "FDSO"))
          {
            MoveAdministrativeActCertification1(entities.
              AdministrativeActCertification,
              local.Auto.Update.AutoAdministrativeActCertification);
            local.Auto.Update.AutoAdministrativeActCertification.
              CurrentAmountDate =
                entities.AdministrativeActCertification.DateSent;
            MoveAdministrativeActCertification2(entities.FederalDebtSetoff,
              export.Export1.Update.DetailFederalDebtSetoff);

            if (Equal(entities.AdministrativeActCertification.CurrentAmount,
              entities.FederalDebtSetoff.AdcAmount))
            {
              export.Export1.Update.DetailFederalDebtSetoff.NonAdcAmount = 0;
            }
            else if (Equal(entities.AdministrativeActCertification.
              CurrentAmount, entities.FederalDebtSetoff.NonAdcAmount))
            {
              export.Export1.Update.DetailFederalDebtSetoff.AdcAmount = 0;
            }

            export.Export1.Update.DetailAdministrativeActCertification.
              CurrentAmount =
                export.Export1.Item.DetailFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();
          }

          export.Export1.Update.DetailCertfiable.SelectChar = "Y";
        }
      }
    }

    local.Manual.Index = -1;

    if (AsChar(import.ListOptManualAutoActs.OneChar) == 'M' || IsEmpty
      (import.ListOptManualAutoActs.OneChar))
    {
      // ---------------------------------------------------------------
      // Obtaining all the administrative action inforamtion for an
      // obligor. These will be the admin actions that were set up
      // manually  (by worker)
      // ---------------------------------------------------------------
      foreach(var item in ReadObligationAdministrativeAction())
      {
        if (ReadAdministrativeAction())
        {
          if (!IsEmpty(import.Required.Type1))
          {
            if (!Equal(entities.AdministrativeAction.Type1,
              import.Required.Type1))
            {
              continue;
            }
          }

          if (Lt(local.InitializedToZeros.Date, import.StartDate.Date))
          {
            if (!Lt(entities.ObligationAdministrativeAction.TakenDate,
              import.StartDate.Date))
            {
              continue;
            }
          }
        }
        else
        {
          ExitState = "ADMINISTRATIVE_ACTION_NF";

          return;
        }

        if (IsEmpty(import.ListOptManualAutoActs.OneChar))
        {
          ++local.Manual.Index;
          local.Manual.CheckSize();

          local.ManualFlag.Flag = "Y";
          local.Manual.Update.ManualAdministrativeAction.Type1 =
            entities.AdministrativeAction.Type1;
          local.Manual.Update.ManualObligationAdministrativeAction.TakenDate =
            entities.ObligationAdministrativeAction.TakenDate;
          local.Manual.Update.ManualCommon.SelectChar = "N";

          if (local.Manual.Index + 1 >= Local.ManualGroup.Capacity)
          {
            goto Test1;
          }
        }
        else
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.DetailAdministrativeAction.Type1 =
            entities.AdministrativeAction.Type1;
          export.Export1.Update.DetailObligationAdministrativeAction.TakenDate =
            entities.ObligationAdministrativeAction.TakenDate;
          export.Export1.Update.DetailCertfiable.SelectChar = "N";

          if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
          {
            return;
          }
        }
      }
    }

Test1:

    local.Auto.Index = 0;
    local.Auto.CheckSize();

    local.Manual.Index = 0;
    local.Manual.CheckSize();

    if (AsChar(local.AutoFlag.Flag) == 'Y' && AsChar(local.ManualFlag.Flag) == 'Y'
      )
    {
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (Lt(local.Manual.Item.ManualObligationAdministrativeAction.TakenDate,
          local.Auto.Item.AutoAdministrativeActCertification.TakenDate))
        {
          export.Export1.Update.DetailAdministrativeActCertification.Assign(
            local.Auto.Item.AutoAdministrativeActCertification);
          MoveAdministrativeActCertification2(local.Auto.Item.
            AutoFederalDebtSetoff,
            export.Export1.Update.DetailFederalDebtSetoff);
          export.Export1.Update.DetailAdministrativeAction.Type1 =
            local.Auto.Item.AutoAdministrativeAction.Type1;
          export.Export1.Update.DetailCertfiable.SelectChar =
            local.Auto.Item.AutoCommon.SelectChar;
          export.Export1.Update.DetailObligationAdministrativeAction.TakenDate =
            local.Auto.Item.AutoObligationAdministrativeAction.TakenDate;

          ++local.Auto.Index;
          local.Auto.CheckSize();

          continue;
        }

        if (Lt(local.Auto.Item.AutoAdministrativeActCertification.TakenDate,
          local.Manual.Item.ManualObligationAdministrativeAction.TakenDate))
        {
          export.Export1.Update.DetailAdministrativeActCertification.Assign(
            local.Manual.Item.ManualAdministrativeActCertification);
          export.Export1.Update.DetailAdministrativeAction.Type1 =
            local.Manual.Item.ManualAdministrativeAction.Type1;
          export.Export1.Update.DetailCertfiable.SelectChar =
            local.Manual.Item.ManualCommon.SelectChar;
          export.Export1.Update.DetailObligationAdministrativeAction.TakenDate =
            local.Manual.Item.ManualObligationAdministrativeAction.TakenDate;

          ++local.Manual.Index;
          local.Manual.CheckSize();

          continue;
        }

        if (Equal(local.Auto.Item.AutoAdministrativeActCertification.TakenDate,
          local.Manual.Item.ManualObligationAdministrativeAction.TakenDate))
        {
          if (Equal(local.Auto.Item.AutoAdministrativeActCertification.
            TakenDate, local.InitializedToZeros.Date) && IsEmpty
            (local.Auto.Item.AutoAdministrativeAction.Type1) && Equal
            (local.Manual.Item.ManualObligationAdministrativeAction.TakenDate,
            local.InitializedToZeros.Date) && IsEmpty
            (local.Manual.Item.ManualAdministrativeAction.Type1))
          {
            goto Test2;
          }

          if (Lt(local.Manual.Item.ManualAdministrativeAction.Type1,
            local.Auto.Item.AutoAdministrativeAction.Type1))
          {
            export.Export1.Update.DetailAdministrativeActCertification.Assign(
              local.Auto.Item.AutoAdministrativeActCertification);
            MoveAdministrativeActCertification2(local.Auto.Item.
              AutoFederalDebtSetoff,
              export.Export1.Update.DetailFederalDebtSetoff);
            export.Export1.Update.DetailAdministrativeAction.Type1 =
              local.Auto.Item.AutoAdministrativeAction.Type1;
            export.Export1.Update.DetailCertfiable.SelectChar =
              local.Auto.Item.AutoCommon.SelectChar;
            export.Export1.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.Auto.Item.AutoObligationAdministrativeAction.TakenDate;

            ++local.Auto.Index;
            local.Auto.CheckSize();

            continue;
          }

          if (Lt(local.Auto.Item.AutoAdministrativeAction.Type1,
            local.Manual.Item.ManualAdministrativeAction.Type1))
          {
            export.Export1.Update.DetailAdministrativeActCertification.Assign(
              local.Manual.Item.ManualAdministrativeActCertification);
            export.Export1.Update.DetailAdministrativeAction.Type1 =
              local.Manual.Item.ManualAdministrativeAction.Type1;
            export.Export1.Update.DetailCertfiable.SelectChar =
              local.Manual.Item.ManualCommon.SelectChar;
            export.Export1.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.Manual.Item.ManualObligationAdministrativeAction.
                TakenDate;

            ++local.Manual.Index;
            local.Manual.CheckSize();

            continue;
          }
        }
      }

      export.Export1.CheckIndex();
    }
    else if (AsChar(local.AutoFlag.Flag) == 'Y' && AsChar
      (local.ManualFlag.Flag) == 'N')
    {
      for(local.Auto.Index = 0; local.Auto.Index < local.Auto.Count; ++
        local.Auto.Index)
      {
        if (!local.Auto.CheckSize())
        {
          break;
        }

        export.Export1.Index = local.Auto.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailAdministrativeActCertification.Assign(
          local.Auto.Item.AutoAdministrativeActCertification);
        MoveAdministrativeActCertification2(local.Auto.Item.
          AutoFederalDebtSetoff, export.Export1.Update.DetailFederalDebtSetoff);
          
        export.Export1.Update.DetailAdministrativeAction.Type1 =
          local.Auto.Item.AutoAdministrativeAction.Type1;
        export.Export1.Update.DetailCertfiable.SelectChar =
          local.Auto.Item.AutoCommon.SelectChar;
        export.Export1.Update.DetailObligationAdministrativeAction.TakenDate =
          local.Auto.Item.AutoObligationAdministrativeAction.TakenDate;
      }

      local.Auto.CheckIndex();
    }
    else if (AsChar(local.AutoFlag.Flag) == 'N' && AsChar
      (local.ManualFlag.Flag) == 'Y')
    {
      for(local.Manual.Index = 0; local.Manual.Index < local.Manual.Count; ++
        local.Manual.Index)
      {
        if (!local.Manual.CheckSize())
        {
          break;
        }

        export.Export1.Index = local.Manual.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailAdministrativeActCertification.Assign(
          local.Manual.Item.ManualAdministrativeActCertification);
        export.Export1.Update.DetailAdministrativeAction.Type1 =
          local.Manual.Item.ManualAdministrativeAction.Type1;
        export.Export1.Update.DetailCertfiable.SelectChar =
          local.Manual.Item.ManualCommon.SelectChar;
        export.Export1.Update.DetailObligationAdministrativeAction.TakenDate =
          local.Manual.Item.ManualObligationAdministrativeAction.TakenDate;
      }

      local.Manual.CheckIndex();
    }

Test2:

    if (AsChar(local.AutoFlag.Flag) == 'Y' && AsChar(local.ManualFlag.Flag) == 'Y'
      )
    {
      export.TotalNoOfEntries.Count = export.Export1.Index;
    }
    else
    {
      export.TotalNoOfEntries.Count = export.Export1.Index + 1;
    }
  }

  private static void MoveAdministrativeActCertification1(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.Type1 = source.Type1;
    target.TakenDate = source.TakenDate;
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.CurrentAmountDate = source.CurrentAmountDate;
    target.DecertifiedDate = source.DecertifiedDate;
  }

  private static void MoveAdministrativeActCertification2(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private IEnumerable<bool>
    ReadAdministrativeActCertificationAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;
    entities.AdministrativeActCertification.Populated = false;

    return ReadEach("ReadAdministrativeActCertificationAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAction.Type1 = db.GetString(reader, 4);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeActCertification.NotificationDate =
          db.GetNullableDate(reader, 9);
        entities.AdministrativeActCertification.CreatedBy =
          db.GetString(reader, 10);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 11);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.AdministrativeActCertification.CseCentralOfficeApprovalDate =
          db.GetNullableDate(reader, 14);
        entities.AdministrativeActCertification.NotifiedBy =
          db.GetNullableString(reader, 15);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 16);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 17);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAction.Description = db.GetString(reader, 19);
        entities.AdministrativeAction.CreatedBy = db.GetString(reader, 20);
        entities.AdministrativeAction.CreatedTstamp =
          db.GetDateTime(reader, 21);
        entities.AdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.AdministrativeAction.Populated = true;
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);

        return true;
      });
  }

  private bool ReadAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.ObligationAdministrativeAction.Populated);
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationAdministrativeAction.AatType);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.CreatedBy = db.GetString(reader, 2);
        entities.AdministrativeAction.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.AdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadFederalDebtSetoff()
  {
    entities.FederalDebtSetoff.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAction.Type1);
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetString(
          command, "tanfCode",
          entities.AdministrativeActCertification.TanfCode);
      },
      (db, reader) =>
      {
        entities.FederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.FederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.FederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.FederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.FederalDebtSetoff.AatType = db.GetNullableString(reader, 4);
        entities.FederalDebtSetoff.AdcAmount = db.GetNullableDecimal(reader, 5);
        entities.FederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 6);
        entities.FederalDebtSetoff.TanfCode = db.GetString(reader, 7);
        entities.FederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.FederalDebtSetoff.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.FederalDebtSetoff.Type1);
      });
  }

  private IEnumerable<bool> ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return ReadEach("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ObligationAdministrativeAction.ResponseDate =
          db.GetNullableDate(reader, 6);
        entities.ObligationAdministrativeAction.CreatedBy =
          db.GetString(reader, 7);
        entities.ObligationAdministrativeAction.CreatedTstamp =
          db.GetDateTime(reader, 8);
        entities.ObligationAdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ObligationAdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationAdministrativeAction.Response =
          db.GetNullableString(reader, 11);
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);

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
    /// A value of ListOptManualAutoActs.
    /// </summary>
    [JsonPropertyName("listOptManualAutoActs")]
    public Standard ListOptManualAutoActs
    {
      get => listOptManualAutoActs ??= new();
      set => listOptManualAutoActs = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private Standard listOptManualAutoActs;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private CsePerson obligor;
    private Obligation obligation;
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
      /// A value of DetailCertfiable.
      /// </summary>
      [JsonPropertyName("detailCertfiable")]
      public Common DetailCertfiable
      {
        get => detailCertfiable ??= new();
        set => detailCertfiable = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCertfiable;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
      private AdministrativeAction detailAdministrativeAction;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    /// <summary>
    /// A value of TotalNoOfEntries.
    /// </summary>
    [JsonPropertyName("totalNoOfEntries")]
    public Common TotalNoOfEntries
    {
      get => totalNoOfEntries ??= new();
      set => totalNoOfEntries = value;
    }

    private Array<ExportGroup> export1;
    private Common totalNoOfEntries;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ManualGroup group.</summary>
    [Serializable]
    public class ManualGroup
    {
      /// <summary>
      /// A value of ManualCommon.
      /// </summary>
      [JsonPropertyName("manualCommon")]
      public Common ManualCommon
      {
        get => manualCommon ??= new();
        set => manualCommon = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("manualAdministrativeActCertification")]
      public AdministrativeActCertification ManualAdministrativeActCertification
      {
        get => manualAdministrativeActCertification ??= new();
        set => manualAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of ManualFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("manualFederalDebtSetoff")]
      public AdministrativeActCertification ManualFederalDebtSetoff
      {
        get => manualFederalDebtSetoff ??= new();
        set => manualFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of ManualObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualObligationAdministrativeAction")]
      public ObligationAdministrativeAction ManualObligationAdministrativeAction
      {
        get => manualObligationAdministrativeAction ??= new();
        set => manualObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualAdministrativeAction")]
      public AdministrativeAction ManualAdministrativeAction
      {
        get => manualAdministrativeAction ??= new();
        set => manualAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common manualCommon;
      private AdministrativeActCertification manualAdministrativeActCertification;
        
      private AdministrativeActCertification manualFederalDebtSetoff;
      private ObligationAdministrativeAction manualObligationAdministrativeAction;
        
      private AdministrativeAction manualAdministrativeAction;
    }

    /// <summary>A AutoGroup group.</summary>
    [Serializable]
    public class AutoGroup
    {
      /// <summary>
      /// A value of AutoCommon.
      /// </summary>
      [JsonPropertyName("autoCommon")]
      public Common AutoCommon
      {
        get => autoCommon ??= new();
        set => autoCommon = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("autoAdministrativeActCertification")]
      public AdministrativeActCertification AutoAdministrativeActCertification
      {
        get => autoAdministrativeActCertification ??= new();
        set => autoAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of AutoFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("autoFederalDebtSetoff")]
      public AdministrativeActCertification AutoFederalDebtSetoff
      {
        get => autoFederalDebtSetoff ??= new();
        set => autoFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of AutoObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoObligationAdministrativeAction")]
      public ObligationAdministrativeAction AutoObligationAdministrativeAction
      {
        get => autoObligationAdministrativeAction ??= new();
        set => autoObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoAdministrativeAction")]
      public AdministrativeAction AutoAdministrativeAction
      {
        get => autoAdministrativeAction ??= new();
        set => autoAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common autoCommon;
      private AdministrativeActCertification autoAdministrativeActCertification;
      private AdministrativeActCertification autoFederalDebtSetoff;
      private ObligationAdministrativeAction autoObligationAdministrativeAction;
      private AdministrativeAction autoAdministrativeAction;
    }

    /// <summary>
    /// A value of ZdelLocalAacoTimestamp.
    /// </summary>
    [JsonPropertyName("zdelLocalAacoTimestamp")]
    public Common ZdelLocalAacoTimestamp
    {
      get => zdelLocalAacoTimestamp ??= new();
      set => zdelLocalAacoTimestamp = value;
    }

    /// <summary>
    /// A value of ManualFlag.
    /// </summary>
    [JsonPropertyName("manualFlag")]
    public Common ManualFlag
    {
      get => manualFlag ??= new();
      set => manualFlag = value;
    }

    /// <summary>
    /// A value of AutoFlag.
    /// </summary>
    [JsonPropertyName("autoFlag")]
    public Common AutoFlag
    {
      get => autoFlag ??= new();
      set => autoFlag = value;
    }

    /// <summary>
    /// A value of CredAlreadyMoved.
    /// </summary>
    [JsonPropertyName("credAlreadyMoved")]
    public Common CredAlreadyMoved
    {
      get => credAlreadyMoved ??= new();
      set => credAlreadyMoved = value;
    }

    /// <summary>
    /// A value of FdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("fdsoAlreadyMoved")]
    public Common FdsoAlreadyMoved
    {
      get => fdsoAlreadyMoved ??= new();
      set => fdsoAlreadyMoved = value;
    }

    /// <summary>
    /// A value of SdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("sdsoAlreadyMoved")]
    public Common SdsoAlreadyMoved
    {
      get => sdsoAlreadyMoved ??= new();
      set => sdsoAlreadyMoved = value;
    }

    /// <summary>
    /// Gets a value of Manual.
    /// </summary>
    [JsonIgnore]
    public Array<ManualGroup> Manual => manual ??= new(ManualGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Manual for json serialization.
    /// </summary>
    [JsonPropertyName("manual")]
    [Computed]
    public IList<ManualGroup> Manual_Json
    {
      get => manual;
      set => Manual.Assign(value);
    }

    /// <summary>
    /// Gets a value of Auto.
    /// </summary>
    [JsonIgnore]
    public Array<AutoGroup> Auto => auto ??= new(AutoGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Auto for json serialization.
    /// </summary>
    [JsonPropertyName("auto")]
    [Computed]
    public IList<AutoGroup> Auto_Json
    {
      get => auto;
      set => Auto.Assign(value);
    }

    /// <summary>
    /// A value of InitializedToZeros.
    /// </summary>
    [JsonPropertyName("initializedToZeros")]
    public DateWorkArea InitializedToZeros
    {
      get => initializedToZeros ??= new();
      set => initializedToZeros = value;
    }

    private Common zdelLocalAacoTimestamp;
    private Common manualFlag;
    private Common autoFlag;
    private Common credAlreadyMoved;
    private Common fdsoAlreadyMoved;
    private Common sdsoAlreadyMoved;
    private Array<ManualGroup> manual;
    private Array<AutoGroup> auto;
    private DateWorkArea initializedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public AdminActionCertObligation Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public AdministrativeActCertification Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private AdministrativeAction administrativeAction;
    private AdminActionCertObligation zdel;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeActCertification federalDebtSetoff;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private AdministrativeActCertification existing;
  }
#endregion
}
