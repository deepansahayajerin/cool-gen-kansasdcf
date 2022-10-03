// Program: LE_LACS_LIST_LEG_ACTS_BY_CSE_CAS, ID: 371972419, model: 746.
// Short name: SWE01676
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
/// A program: LE_LACS_LIST_LEG_ACTS_BY_CSE_CAS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will List Legal Actions by CSE Case Number.
/// This was the original action block used by LCCC. It has been superseded.
/// Dont delete this acblk
/// </para>
/// </summary>
[Serializable]
public partial class LeLacsListLegActsByCseCas: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACS_LIST_LEG_ACTS_BY_CSE_CAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLacsListLegActsByCseCas(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLacsListLegActsByCseCas.
  /// </summary>
  public LeLacsListLegActsByCseCas(IContext context, Import import,
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
    // ****************************************************************
    // Modify code for listing legal actions prior to cse case opening.
    // ****************************************************************
    // -----------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/10/96  Govind			Initial coding
    // 11/18/98  P McElderry	None listed	Removed ununsed entity views and READs;
    // enhanced READs.
    // 04/02/02  GVandy	PR# 138221	Read for end dated code values when 
    // retrieving
    // 					action taken description.
    // 12/23/02  GVandy	WR10492		Read and export attribute system_gen_ind.
    // 06/16/11  T. Pierce	CQ23493		Changes to support new scrolling mechanism.
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------
    // This action block supersedes the action block that was used
    // by LACS: LE LIST LEG ACTS BY CSE. The original acblk
    // returned all the legal actions, some of which may not be
    // relevant to the cse case at all.
    // ------------------------------------------------------------------
    export.CutOff.Date = import.CutOff.Date;

    if (Equal(import.CutOff.Date, null))
    {
      export.CutOff.Date = UseCabSetMaximumDiscontinueDate();
    }

    export.List.Index = -1;

    // ------------------------------------------------------
    // CQ23493  06/16/2011  T. Pierce
    // Action block changed to provide support for
    // scrolling requirements of calling procedure step.
    // ------------------------------------------------------
    if (AsChar(import.CourtCaseNumberOnly.Text1) == 'Y' || IsEmpty
      (import.CourtCaseNumberOnly.Text1))
    {
      // ---------------------------------
      //  List only the Court Case Numbers
      // ---------------------------------
      switch(TrimEnd(global.Command))
      {
        case "DISPLAY":
          foreach(var item in ReadLegalActionTribunal1())
          {
            // ------------------------------------------------------------------
            // If only distinct court case nos are required, skip the
            // subsequent records with the same court case number;
            // otherwise skip the subsequent records with the same legal
            // action identifier
            // -------------------------------------------------------------------
            if (Equal(entities.LegalAction.CourtCaseNumber,
              local.PreviousLegalAction.CourtCaseNumber))
            {
              if (entities.ExistingTribunal.Identifier == local
                .PreviousTribunal.Identifier)
              {
                continue;
              }
            }

            ++export.List.Index;
            export.List.CheckSize();

            if (export.List.Index >= Export.ListGroup.Capacity)
            {
              export.More.Flag = "Y";

              return;
            }
            else if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
              export.NextCourtCaseNumOnlyTribunal.Identifier =
                entities.ExistingTribunal.Identifier;
            }

            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);
            local.PreviousTribunal.Identifier =
              entities.ExistingTribunal.Identifier;

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);
          }

          break;
        case "NEXT":
          export.Prev.Flag = "Y";

          foreach(var item in ReadLegalActionTribunal2())
          {
            if (Equal(entities.LegalAction.CourtCaseNumber,
              import.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber))
            {
              if (entities.ExistingTribunal.Identifier <= import
                .StartCourtCaseNumOnlyTribunal.Identifier)
              {
                continue;
              }
            }

            // ------------------------------------------------------------------
            // If only distinct court case nos are required, skip the
            // subsequent records with the same court case number;
            // otherwise skip the subsequent records with the same legal
            // action identifier
            // -------------------------------------------------------------------
            if (Equal(entities.LegalAction.CourtCaseNumber,
              local.PreviousLegalAction.CourtCaseNumber))
            {
              if (entities.ExistingTribunal.Identifier == local
                .PreviousTribunal.Identifier)
              {
                continue;
              }
            }

            ++export.List.Index;
            export.List.CheckSize();

            if (export.List.Index >= Export.ListGroup.Capacity)
            {
              export.More.Flag = "Y";

              return;
            }
            else if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
              export.NextCourtCaseNumOnlyTribunal.Identifier =
                entities.ExistingTribunal.Identifier;
            }

            if (export.List.Index == 0)
            {
              export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
              export.PrevCourtCaseNumOnlyTribunal.Identifier =
                entities.ExistingTribunal.Identifier;
            }

            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);
            local.PreviousTribunal.Identifier =
              entities.ExistingTribunal.Identifier;

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);
          }

          break;
        case "PREV":
          export.List.Index = Export.ListGroup.Capacity;
          export.List.CheckSize();

          export.List.Count = Export.ListGroup.Capacity;
          export.More.Flag = "Y";

          foreach(var item in ReadLegalActionTribunal3())
          {
            if (Equal(entities.LegalAction.CourtCaseNumber,
              import.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber))
            {
              if (entities.ExistingTribunal.Identifier >= import
                .StartCourtCaseNumOnlyTribunal.Identifier)
              {
                continue;
              }
            }

            // ------------------------------------------------------------------
            // If only distinct court case nos are required, skip the
            // subsequent records with the same court case number;
            // otherwise skip the subsequent records with the same legal
            // action identifier
            // -------------------------------------------------------------------
            if (Equal(entities.LegalAction.CourtCaseNumber,
              local.PreviousLegalAction.CourtCaseNumber))
            {
              if (entities.ExistingTribunal.Identifier == local
                .PreviousTribunal.Identifier)
              {
                continue;
              }
            }

            --export.List.Index;
            export.List.CheckSize();

            if (export.List.Index == -1)
            {
              export.Prev.Flag = "Y";

              export.List.Index = 0;
              export.List.CheckSize();

              export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
                export.List.Item.DetailLegalAction.CourtCaseNumber;
              export.PrevCourtCaseNumOnlyTribunal.Identifier =
                export.List.Item.DetailTribunal.Identifier;

              return;
            }

            if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
              export.NextCourtCaseNumOnlyTribunal.Identifier =
                entities.ExistingTribunal.Identifier;
            }

            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);
            local.PreviousTribunal.Identifier =
              entities.ExistingTribunal.Identifier;

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);
          }

          break;
        default:
          break;
      }
    }
    else
    {
      // --------------------------
      // List all the Legal Actions
      // ---------------------------
      switch(TrimEnd(global.Command))
      {
        case "DISPLAY":
          foreach(var item in ReadLegalActionTribunalLegalActionCaseRole3())
          {
            if (!Lt(Date(entities.LegalAction.CreatedTstamp), export.CutOff.Date)
              || AsChar(import.ListLactsPriorToCase.Flag) != 'Y' && AsChar
              (entities.LegalActionCaseRole.InitialCreationInd) != 'Y' || !
              IsEmpty(import.Filter.CourtCaseNumber) && Lt
              (entities.LegalAction.CourtCaseNumber,
              import.Filter.CourtCaseNumber) || !
              IsEmpty(import.Filter.Classification) && AsChar
              (entities.LegalAction.Classification) != AsChar
              (import.Filter.Classification))
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            if (entities.LegalAction.Identifier == local
              .PreviousLegalAction.Identifier)
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            ++export.List.Index;
            export.List.CheckSize();

            if (export.List.Index >= Export.ListGroup.Capacity)
            {
              export.More.Flag = "Y";

              return;
            }
            else if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              MoveLegalAction2(entities.LegalAction, export.NextAllActions);
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);

            if (ReadLegalActionAppeal())
            {
              export.List.Update.DetailLaAppInd.Flag = "Y";
            }
            else
            {
              // -------------------------------------------------------
              // Continue processing - not every LEGAL_ACTION will have
              // an appeal
              // --------------------------------------------------------
            }

            UseLeGetActionTakenDescription();
            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }
          }

          break;
        case "NEXT":
          foreach(var item in ReadLegalActionTribunalLegalActionCaseRole2())
          {
            if (Equal(entities.LegalAction.CreatedTstamp,
              import.StartAllActions.CreatedTstamp))
            {
              if (entities.LegalAction.Identifier >= import
                .StartAllActions.Identifier)
              {
                continue;
              }
            }

            if (!Lt(Date(entities.LegalAction.CreatedTstamp), export.CutOff.Date)
              || AsChar(import.ListLactsPriorToCase.Flag) != 'Y' && AsChar
              (entities.LegalActionCaseRole.InitialCreationInd) != 'Y' || !
              IsEmpty(import.Filter.CourtCaseNumber) && Lt
              (entities.LegalAction.CourtCaseNumber,
              import.Filter.CourtCaseNumber) || !
              IsEmpty(import.Filter.Classification) && AsChar
              (entities.LegalAction.Classification) != AsChar
              (import.Filter.Classification))
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            if (entities.LegalAction.Identifier == local
              .PreviousLegalAction.Identifier)
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            ++export.List.Index;
            export.List.CheckSize();

            if (export.List.Index >= Export.ListGroup.Capacity)
            {
              export.More.Flag = "Y";

              return;
            }
            else if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              MoveLegalAction2(entities.LegalAction, export.NextAllActions);
            }

            if (export.List.Index == 0)
            {
              export.Prev.Flag = "Y";
              MoveLegalAction2(entities.LegalAction, export.PrevAllActions);
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);

            if (ReadLegalActionAppeal())
            {
              export.List.Update.DetailLaAppInd.Flag = "Y";
            }
            else
            {
              // -------------------------------------------------------
              // Continue processing - not every LEGAL_ACTION will have
              // an appeal
              // --------------------------------------------------------
            }

            UseLeGetActionTakenDescription();
            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }
          }

          break;
        case "PREV":
          export.List.Index = Export.ListGroup.Capacity;
          export.List.CheckSize();

          export.List.Count = Export.ListGroup.Capacity;
          export.More.Flag = "Y";

          foreach(var item in ReadLegalActionTribunalLegalActionCaseRole1())
          {
            if (Equal(entities.LegalAction.CreatedTstamp,
              import.StartAllActions.CreatedTstamp))
            {
              if (entities.LegalAction.Identifier <= import
                .StartAllActions.Identifier)
              {
                continue;
              }
            }

            if (!Lt(Date(entities.LegalAction.CreatedTstamp), export.CutOff.Date)
              || AsChar(import.ListLactsPriorToCase.Flag) != 'Y' && AsChar
              (entities.LegalActionCaseRole.InitialCreationInd) != 'Y' || !
              IsEmpty(import.Filter.CourtCaseNumber) && Lt
              (entities.LegalAction.CourtCaseNumber,
              import.Filter.CourtCaseNumber) || !
              IsEmpty(import.Filter.Classification) && AsChar
              (entities.LegalAction.Classification) != AsChar
              (import.Filter.Classification))
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            if (entities.LegalAction.Identifier == local
              .PreviousLegalAction.Identifier)
            {
              continue;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }

            --export.List.Index;
            export.List.CheckSize();

            if (export.List.Index == -1)
            {
              export.Prev.Flag = "Y";

              export.List.Index = 0;
              export.List.CheckSize();

              MoveLegalAction2(export.List.Item.DetailLegalAction,
                export.PrevAllActions);

              return;
            }

            if (export.List.Index + 1 == Export.ListGroup.Capacity)
            {
              MoveLegalAction2(entities.LegalAction, export.NextAllActions);
            }

            MoveLegalAction1(entities.LegalAction, local.PreviousLegalAction);
            export.List.Update.DetailLegalAction.Assign(entities.LegalAction);

            if (ReadLegalActionAppeal())
            {
              export.List.Update.DetailLaAppInd.Flag = "Y";
            }
            else
            {
              // -------------------------------------------------------
              // Continue processing - not every LEGAL_ACTION will have
              // an appeal
              // --------------------------------------------------------
            }

            UseLeGetActionTakenDescription();
            export.List.Update.DetailTribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.List.Update.DetailFips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.List.Update.DetailForeign.Country =
                entities.ExistingForeign.Country;
            }
            else
            {
              // ---------------------
              // Continue processing
              // ---------------------
            }
          }

          break;
        default:
          break;
      }
    }
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = entities.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.List.Update.DetailActionTaken.Description =
      useExport.CodeValue.Description;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingForeign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingForeign.Identifier = db.GetInt32(reader, 0);
        entities.ExistingForeign.Country = db.GetNullableString(reader, 1);
        entities.ExistingForeign.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingForeign.Populated = true;
      });
  }

  private bool ReadLegalActionAppeal()
  {
    entities.ExistingLegalActionAppeal.Populated = false;

    return Read("ReadLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionAppeal.AplId = db.GetInt32(reader, 1);
        entities.ExistingLegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.ExistingLegalActionAppeal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal1()
  {
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.Filter.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal2()
  {
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
        db.SetNullableString(
          command, "courtCaseNo",
          import.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal3()
  {
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
        db.SetNullableString(
          command, "courtCaseNo",
          import.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalLegalActionCaseRole1()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.StartAllActions.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 15);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 16);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 17);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 18);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 19);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 20);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 21);
        entities.LegalActionCaseRole.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalLegalActionCaseRole2()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.StartAllActions.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 15);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 16);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 17);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 18);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 19);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 20);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 21);
        entities.LegalActionCaseRole.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalLegalActionCaseRole3()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalLegalActionCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Search.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 8);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.Name = db.GetString(reader, 10);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 15);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 16);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 17);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 18);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 19);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 20);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 21);
        entities.LegalActionCaseRole.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

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
    /// A value of StartAllActions.
    /// </summary>
    [JsonPropertyName("startAllActions")]
    public LegalAction StartAllActions
    {
      get => startAllActions ??= new();
      set => startAllActions = value;
    }

    /// <summary>
    /// A value of StartCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("startCourtCaseNumOnlyTribunal")]
    public Tribunal StartCourtCaseNumOnlyTribunal
    {
      get => startCourtCaseNumOnlyTribunal ??= new();
      set => startCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of StartCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("startCourtCaseNumOnlyLegalAction")]
    public LegalAction StartCourtCaseNumOnlyLegalAction
    {
      get => startCourtCaseNumOnlyLegalAction ??= new();
      set => startCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of ListLactsPriorToCase.
    /// </summary>
    [JsonPropertyName("listLactsPriorToCase")]
    public Common ListLactsPriorToCase
    {
      get => listLactsPriorToCase ??= new();
      set => listLactsPriorToCase = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of CutOff.
    /// </summary>
    [JsonPropertyName("cutOff")]
    public DateWorkArea CutOff
    {
      get => cutOff ??= new();
      set => cutOff = value;
    }

    /// <summary>
    /// A value of CourtCaseNumberOnly.
    /// </summary>
    [JsonPropertyName("courtCaseNumberOnly")]
    public WorkArea CourtCaseNumberOnly
    {
      get => courtCaseNumberOnly ??= new();
      set => courtCaseNumberOnly = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Case1 Search
    {
      get => search ??= new();
      set => search = value;
    }

    private LegalAction startAllActions;
    private Tribunal startCourtCaseNumOnlyTribunal;
    private LegalAction startCourtCaseNumOnlyLegalAction;
    private Common listLactsPriorToCase;
    private LegalAction filter;
    private DateWorkArea cutOff;
    private WorkArea courtCaseNumberOnly;
    private Case1 search;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailActionTaken.
      /// </summary>
      [JsonPropertyName("detailActionTaken")]
      public CodeValue DetailActionTaken
      {
        get => detailActionTaken ??= new();
        set => detailActionTaken = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailLaAppInd;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private CodeValue detailActionTaken;
      private Tribunal detailTribunal;
      private Common detailCommon;
      private LegalAction detailLegalAction;
    }

    /// <summary>
    /// A value of PrevAllActions.
    /// </summary>
    [JsonPropertyName("prevAllActions")]
    public LegalAction PrevAllActions
    {
      get => prevAllActions ??= new();
      set => prevAllActions = value;
    }

    /// <summary>
    /// A value of NextAllActions.
    /// </summary>
    [JsonPropertyName("nextAllActions")]
    public LegalAction NextAllActions
    {
      get => nextAllActions ??= new();
      set => nextAllActions = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyTribunal")]
    public Tribunal PrevCourtCaseNumOnlyTribunal
    {
      get => prevCourtCaseNumOnlyTribunal ??= new();
      set => prevCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyLegalAction")]
    public LegalAction PrevCourtCaseNumOnlyLegalAction
    {
      get => prevCourtCaseNumOnlyLegalAction ??= new();
      set => prevCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Common Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyTribunal")]
    public Tribunal NextCourtCaseNumOnlyTribunal
    {
      get => nextCourtCaseNumOnlyTribunal ??= new();
      set => nextCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyLegalAction")]
    public LegalAction NextCourtCaseNumOnlyLegalAction
    {
      get => nextCourtCaseNumOnlyLegalAction ??= new();
      set => nextCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of CutOff.
    /// </summary>
    [JsonPropertyName("cutOff")]
    public DateWorkArea CutOff
    {
      get => cutOff ??= new();
      set => cutOff = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    private LegalAction prevAllActions;
    private LegalAction nextAllActions;
    private Tribunal prevCourtCaseNumOnlyTribunal;
    private LegalAction prevCourtCaseNumOnlyLegalAction;
    private Common prev;
    private Common more;
    private Tribunal nextCourtCaseNumOnlyTribunal;
    private LegalAction nextCourtCaseNumOnlyLegalAction;
    private DateWorkArea cutOff;
    private Array<ListGroup> list;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PreviousFips.
    /// </summary>
    [JsonPropertyName("previousFips")]
    public Fips PreviousFips
    {
      get => previousFips ??= new();
      set => previousFips = value;
    }

    /// <summary>
    /// A value of PreviousTribunal.
    /// </summary>
    [JsonPropertyName("previousTribunal")]
    public Tribunal PreviousTribunal
    {
      get => previousTribunal ??= new();
      set => previousTribunal = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public LegalAction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    private Fips previousFips;
    private Tribunal previousTribunal;
    private LegalAction null1;
    private LegalAction previousLegalAction;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("existingLegalActionAppeal")]
    public LegalActionAppeal ExistingLegalActionAppeal
    {
      get => existingLegalActionAppeal ??= new();
      set => existingLegalActionAppeal = value;
    }

    /// <summary>
    /// A value of ExistingForeign.
    /// </summary>
    [JsonPropertyName("existingForeign")]
    public FipsTribAddress ExistingForeign
    {
      get => existingForeign ??= new();
      set => existingForeign = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionAppeal existingLegalActionAppeal;
    private FipsTribAddress existingForeign;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalAction legalAction;
  }
#endregion
}
