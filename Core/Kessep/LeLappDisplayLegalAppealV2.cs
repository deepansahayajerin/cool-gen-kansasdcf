// Program: LE_LAPP_DISPLAY_LEGAL_APPEAL_V2, ID: 371973993, model: 746.
// Short name: SWE01625
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LAPP_DISPLAY_LEGAL_APPEAL_V2.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and populates APPEAL for display
/// </para>
/// </summary>
[Serializable]
public partial class LeLappDisplayLegalAppealV2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_DISPLAY_LEGAL_APPEAL_V2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappDisplayLegalAppealV2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappDisplayLegalAppealV2.
  /// </summary>
  public LeLappDisplayLegalAppealV2(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Change log
    // Date		Description
    // Developer
    // 12/14/98	P McElderry
    // Restructured code to bypass READs in certain situations.
    // Rewrote READ EACHes eliminating entity views which
    // already had values verified and populated.
    // Eliminated similar READ statements.
    // 12/17/98	P McElderry
    // Fixed scrolling logic, changed display logic to read from
    // acending cts.
    // --------------------------------------------------------------
    // -----------------------------------------------------------
    // The display function will be based on a given appeal docket
    // number for the given court case.
    // Prev/Next will display the previous appeal with the same
    // appeal docket number for the given legal action.
    // It is possible that there are two appeals against the same
    // legal action (one by cse and one by AP). Both the appeals
    // will have the same appeal document number. So the above
    // logic will work.
    // It is also possible that one appeal may be filed against
    // more than one legal action. Let us assume a situation as
    // below.
    // Legal Action   Appealed by   APPEAL Docket #
    //    LA1 		CSE		APD1
    //    LA2 		CSE		APD1
    //    LA1 		AP		APD1
    //    LA3 		AP		APD1
    // All the above four will have created 4 different APPEAL
    // records with the same appeal document number. The above
    // logic will work and display the four APPEAL records created
    // one by one.
    // It is unlikely that totally different (unrelated) appeals exist
    // with the same appeal docket number across different legal
    // actions under the same court case. In other words, if the
    // appeal docket number is the same for two APPEAL records,
    // then the two APPEAL records must be related appeals.
    // It is not possible to appeal more than once against the
    // same legal action over time. So we dont have to take care of
    // that situation.
    // ----------------------------------------------------------------
    export.AppealedAgainst.Assign(import.AppealedTo);
    export.AppealedFrom.Assign(import.From);
    export.AppealedTo.Assign(import.To);
    MoveAppeal(import.Appeal, export.Appeal);

    // --------------------------------------------------------------
    // treat the import legal action as one of the legal actions in
    // the same court case.
    // This will help in cases where appeal docket number is
    // blank. Where the appeal docket no is non blank, we have no
    // problem displaying correct prev and next appeals. Where it
    // is blank, we should be able to display only those that
    // belong to the same court case number.
    // ----------------------------------------------------------------
    // -------------------------------------------------------
    // These READs will be performed only when returning from
    // LACN - otherwise skip them as the needed values will be
    // passed in from the PRAD
    // -------------------------------------------------------
    if (!IsEmpty(import.Retlacn.Command))
    {
      if (ReadLegalAction())
      {
        export.AppealedAgainst.Assign(entities.ExistingOneOfLactInCtCase);
        local.Code.CodeName = "ACTION TAKEN";
        local.CodeValue.Cdvalue = export.AppealedAgainst.ActionTaken;
        UseCabGetCodeValueDescription();

        if (ReadTribunal1())
        {
          export.AppealedFrom.Assign(entities.ExistingAppealedFrom);
        }
        else
        {
          ExitState = "TRIBUNAL_NF";
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
      }
    }
    else
    {
      // ---------------------
      // Data already verified
      // ---------------------
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }
    else
    {
      local.AppealFound.Flag = "N";
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(import.Appeal.DocketNumber))
        {
          if (ReadAppeal4())
          {
            export.Appeal.Assign(entities.ExistingAppeal);
            export.AppealedAgainst.Assign(entities.ExistingAppealedAgainst);
            local.AppealFound.Flag = "Y";
          }

          // ---------------------------------------------------------
          // Did not find an appeal for the given legal action. Return
          // error.
          // ----------------------------------------------------------
        }
        else
        {
          if (ReadAppeal3())
          {
            export.Appeal.Assign(entities.ExistingAppeal);
            export.AppealedAgainst.Assign(entities.ExistingAppealedAgainst);
            local.AppealFound.Flag = "Y";
          }

          // ---------------------------------------------------------
          // Did not find an appeal for the given legal action. Return
          // error.
          // ----------------------------------------------------------
        }

        if (AsChar(local.AppealFound.Flag) == 'N')
        {
          export.Appeal.DocketNumber = "";
          export.Appeal.Identifier = 0;
          ExitState = "APPEAL_NF";

          return;
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }

        break;
      case "NEXT":
        // ---------------------
        // Continue processing
        // ---------------------
        break;
      case "PREV":
        // ---------------------
        // Continue processing
        // ---------------------
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (AsChar(local.AppealFound.Flag) == 'Y' || Equal
      (global.Command, "PREV") || Equal(global.Command, "NEXT"))
    {
      switch(TrimEnd(global.Command))
      {
        case "PREV":
          if (ReadAppeal5())
          {
            export.Appeal.Assign(entities.ExistingAppeal);
            export.ScrollingAttributes.MinusFlag = "-";
            export.ScrollingAttributes.PlusFlag = "+";
          }

          if (entities.ExistingAppeal.Identifier > 0)
          {
            if (ReadAppeal5())
            {
              local.Prev.Identifier = entities.ExistingAppeal.Identifier;
            }

            if (local.Prev.Identifier > 0)
            {
            }
            else
            {
              export.ScrollingAttributes.MinusFlag = "";
            }
          }
          else
          {
            export.ScrollingAttributes.PlusFlag = "+";
            ExitState = "ACO_NE0000_INVALID_BACKWARD";
          }

          local.Code.CodeName = "ACTION TAKEN";
          local.CodeValue.Cdvalue = export.AppealedAgainst.ActionTaken;
          UseCabGetCodeValueDescription();

          break;
        case "NEXT":
          if (ReadAppeal1())
          {
            export.Appeal.Assign(entities.ExistingAppeal);
            export.ScrollingAttributes.PlusFlag = "+";
            export.ScrollingAttributes.MinusFlag = "-";
          }

          if (entities.ExistingAppeal.Identifier > 0)
          {
            if (ReadAppeal1())
            {
              local.Next.Identifier = entities.ExistingAppeal.Identifier;
            }

            if (local.Next.Identifier > 0)
            {
            }
            else
            {
              export.ScrollingAttributes.PlusFlag = "";
            }
          }
          else
          {
            export.ScrollingAttributes.MinusFlag = "-";
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }

          local.Code.CodeName = "ACTION TAKEN";
          local.CodeValue.Cdvalue = export.AppealedAgainst.ActionTaken;
          UseCabGetCodeValueDescription();

          break;
        case "DISPLAY":
          // -------------------
          // Set scrolling flags
          // -------------------
          if (ReadTribunal2())
          {
            export.AppealedTo.Assign(entities.ExistingAppealedTo);

            if (ReadAppeal6())
            {
              export.ScrollingAttributes.MinusFlag = "-";
            }

            if (ReadAppeal2())
            {
              export.ScrollingAttributes.PlusFlag = "+";
            }
          }
          else
          {
            ExitState = "TRIBUNAL_NF";
          }

          break;
        default:
          // ---------------------
          // Only possibilities
          // ---------------------
          break;
      }
    }
  }

  private static void MoveAppeal(Appeal source, Appeal target)
  {
    target.Identifier = source.Identifier;
    target.DocketNumber = source.DocketNumber;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
  }

  private bool ReadAppeal1()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          export.Appeal.CreatedTstamp.GetValueOrDefault());
        db.SetInt32(command, "legalActionId", import.AppealedTo.Identifier);
        db.SetNullableInt32(command, "trbId", export.AppealedFrom.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadAppeal2()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          export.Appeal.CreatedTstamp.GetValueOrDefault());
        db.SetInt32(command, "legalActionId", import.AppealedTo.Identifier);
        db.SetNullableInt32(
          command, "trbId", entities.ExistingAppealedTo.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadAppeal3()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.AppealedTo.Identifier);
        db.SetString(command, "docketNo", import.Appeal.DocketNumber);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadAppeal4()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.AppealedTo.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadAppeal5()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal5",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          export.Appeal.CreatedTstamp.GetValueOrDefault());
        db.SetInt32(command, "legalActionId", import.AppealedTo.Identifier);
        db.SetNullableInt32(command, "trbId", export.AppealedFrom.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadAppeal6()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal6",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          export.Appeal.CreatedTstamp.GetValueOrDefault());
        db.SetInt32(command, "legalActionId", import.AppealedTo.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 22);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 23);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingOneOfLactInCtCase.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.AppealedTo.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingOneOfLactInCtCase.Identifier = db.GetInt32(reader, 0);
        entities.ExistingOneOfLactInCtCase.Classification =
          db.GetString(reader, 1);
        entities.ExistingOneOfLactInCtCase.ActionTaken =
          db.GetString(reader, 2);
        entities.ExistingOneOfLactInCtCase.FiledDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingOneOfLactInCtCase.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingOneOfLactInCtCase.TrbId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingOneOfLactInCtCase.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingOneOfLactInCtCase.Populated);
    entities.ExistingAppealedFrom.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier1",
          entities.ExistingOneOfLactInCtCase.TrbId.GetValueOrDefault());
        db.SetInt32(command, "identifier2", import.From.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppealedFrom.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingAppealedFrom.Name = db.GetString(reader, 1);
        entities.ExistingAppealedFrom.JudicialDistrict =
          db.GetString(reader, 2);
        entities.ExistingAppealedFrom.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAppealedFrom.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.ExistingAppealedTo.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "appealId", export.Appeal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppealedTo.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingAppealedTo.Name = db.GetString(reader, 1);
        entities.ExistingAppealedTo.JudicialDistrict = db.GetString(reader, 2);
        entities.ExistingAppealedTo.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAppealedTo.Populated = true;
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
    /// A value of Retlacn.
    /// </summary>
    [JsonPropertyName("retlacn")]
    public Common Retlacn
    {
      get => retlacn ??= new();
      set => retlacn = value;
    }

    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Tribunal To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Tribunal From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of AppealedTo.
    /// </summary>
    [JsonPropertyName("appealedTo")]
    public LegalAction AppealedTo
    {
      get => appealedTo ??= new();
      set => appealedTo = value;
    }

    private Common retlacn;
    private Appeal appeal;
    private Tribunal to;
    private Tribunal from;
    private LegalAction appealedTo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of AppealedFrom.
    /// </summary>
    [JsonPropertyName("appealedFrom")]
    public Tribunal AppealedFrom
    {
      get => appealedFrom ??= new();
      set => appealedFrom = value;
    }

    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    /// <summary>
    /// A value of AppealedAgainst.
    /// </summary>
    [JsonPropertyName("appealedAgainst")]
    public LegalAction AppealedAgainst
    {
      get => appealedAgainst ??= new();
      set => appealedAgainst = value;
    }

    /// <summary>
    /// A value of AppealedTo.
    /// </summary>
    [JsonPropertyName("appealedTo")]
    public Tribunal AppealedTo
    {
      get => appealedTo ??= new();
      set => appealedTo = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    private CodeValue actionTaken;
    private Tribunal appealedFrom;
    private Appeal appeal;
    private LegalAction appealedAgainst;
    private Tribunal appealedTo;
    private ScrollingAttributes scrollingAttributes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Appeal Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Appeal Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of InitializedToZero.
    /// </summary>
    [JsonPropertyName("initializedToZero")]
    public Appeal InitializedToZero
    {
      get => initializedToZero ??= new();
      set => initializedToZero = value;
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
    /// A value of AppealFound.
    /// </summary>
    [JsonPropertyName("appealFound")]
    public Common AppealFound
    {
      get => appealFound ??= new();
      set => appealFound = value;
    }

    private Appeal next;
    private Appeal prev;
    private Appeal initializedToZero;
    private Code code;
    private CodeValue codeValue;
    private Common appealFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOneOfLactInCtCase.
    /// </summary>
    [JsonPropertyName("existingOneOfLactInCtCase")]
    public LegalAction ExistingOneOfLactInCtCase
    {
      get => existingOneOfLactInCtCase ??= new();
      set => existingOneOfLactInCtCase = value;
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
    /// A value of ExistingAppealedFrom.
    /// </summary>
    [JsonPropertyName("existingAppealedFrom")]
    public Tribunal ExistingAppealedFrom
    {
      get => existingAppealedFrom ??= new();
      set => existingAppealedFrom = value;
    }

    /// <summary>
    /// A value of ExistingAppeal.
    /// </summary>
    [JsonPropertyName("existingAppeal")]
    public Appeal ExistingAppeal
    {
      get => existingAppeal ??= new();
      set => existingAppeal = value;
    }

    /// <summary>
    /// A value of ExistingAppealedAgainst.
    /// </summary>
    [JsonPropertyName("existingAppealedAgainst")]
    public LegalAction ExistingAppealedAgainst
    {
      get => existingAppealedAgainst ??= new();
      set => existingAppealedAgainst = value;
    }

    /// <summary>
    /// A value of ExistingAppealedTo.
    /// </summary>
    [JsonPropertyName("existingAppealedTo")]
    public Tribunal ExistingAppealedTo
    {
      get => existingAppealedTo ??= new();
      set => existingAppealedTo = value;
    }

    private LegalAction existingOneOfLactInCtCase;
    private LegalActionAppeal existingLegalActionAppeal;
    private Tribunal existingAppealedFrom;
    private Appeal existingAppeal;
    private LegalAction existingAppealedAgainst;
    private Tribunal existingAppealedTo;
  }
#endregion
}
