// Program: SP_ODCM_OUTGOING_DOC_MAINTENANCE, ID: 372904911, model: 746.
// Short name: SWEODCMP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ODCM_OUTGOING_DOC_MAINTENANCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOdcmOutgoingDocMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ODCM_OUTGOING_DOC_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOdcmOutgoingDocMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOdcmOutgoingDocMaintenance.
  /// </summary>
  public SpOdcmOutgoingDocMaintenance(IContext context, Import import,
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
    // ---------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ---------------------------------------------------------------
    // 09/11/1995	Siraj Konkader			Initial Dev
    // 03/15/1996	A HACKLER			RETRO FITS
    // 01/03/1997	R Marchman			Add new security/next tran.
    // 08/24/1998	M Ramirez			Post assessment fixes
    // 09/10/1998	M Ramirez			Unit Test Prep
    // 09/21/1998	M Ramirez			Change implicit to explicit
    // 01/29/1999	M Ramirez			Added filters status and canceled
    // 04/06/1999	M Ramirez			Removed sort by Print_successful_ind
    // 04/06/1999	M Ramirez			Renamed screen literal from 'Start
    // 						Date' to 'Document Date'
    // 04/07/1999	M Ramirez			Modified 'Document Date' to translate
    // 						from CCYY-MM-DD-00.00.00.000000
    // 						to CCYY-MM-DD-23.59.59.999999
    // 						(user cannot specify time of day).
    // 06/26/1999	M Ramirez			Fixed scrolling problem
    // 06/26/1999	M Ramirez			Added Doc Name filter
    // 06/26/1999	M Ramirez			Added Case filter
    // 07/16/1999	M Ramirez			Implementation of performance
    // 						recommendations
    // 07/16/1999	M Ramirez			Added Person filter
    // 07/17/1999	M Ramirez			Added prompt to SVPL
    // 07/17/1999	M Ramirez			Added READ of PPI when SP is not found
    // 10/13/1999	PMcElderry			Made "Document" or "User ID" fields
    // 						mandatory for DISPLAY.   Added prompt
    // 						for "Document"
    // 
    // 01/10/2000	Carl Galka 			When only Document Name is entered look
    // 						for one month of data
    // 06/16/2000	M Ramirez	WR# 173 Seg B	Update CSE_Person when user cancels
    // 						FVLTR
    // 02/12/2001	M Ramirez	WR# 187 Seg B	On PRINT, send M's to DDOC instead of 
    // DKEY
    // 06/02/2008	M Fan   	CQ415/PR186740  Fixed the document display
    //                                                 
    // problem (not all of the documents are displayed
    //                                                 
    // for valid display options).
    // ---------------------------------------------------------------
    // 09/19/2008	M Fan   	CQ415/PR186740 -- Seg B (Phase ll )
    //                                                 
    // Changed the maxiumm number of pages displayed
    // from 10 to 99 for
    //                                                 
    // group_import_hidden_pages_keys and
    // group_export_hidden_pages.
    //                                                 
    // 99 is the standard maxiumm number of pages for
    // CSE.
    //                                                 
    // Also, changed the way to calculate local filter
    // minimum outgoing
    //                                                 
    // document last updatd tstamp (when document name
    // is entered and user
    //                                                 
    // id, case and person are not entered) to be sure
    // to display records
    //                                                 
    // correctly.
    // 10/30/2009	J Huss		CQ 12686	Fixed abend when input date filter is less 
    // than 2/2/0001.
    // 09/21/2010	J Huss 		CQ 22087	Emergency fix to improve performance of 
    // screen.
    // 						Adding "or 0 = 1" forces index CKI02777 to be used instead of the 
    // less
    // 						efficient index CKI03777.
    // 09/27/2010	J Huss 		CQ 22190	Emergency fix to improve performance of 
    // screen.
    // 						Adding "or 0 = 1" forces index CKI08777 to be used instead of the 
    // less
    // 						efficient index CKI03419.
    // ------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ConstantZero.Count = 0;
    local.ConstantOne.Count = 1;
    MoveNextTranInfo(import.Hidden, export.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -------------------------------------------------------
    // Move imports to exports
    // -------------------------------------------------------
    local.CurrentDateWorkArea.Date = Now().Date;
    local.CurrentDateWorkArea.Timestamp = Now();
    local.CurrentServiceProvider.UserId = global.UserId;
    export.ServiceProviderName.FormattedName =
      import.ServiceProviderName.FormattedName;
    export.PromptSvpl.SelectChar = import.PromptSvpl.SelectChar;
    export.PromptDocm.SelectChar = import.PromtDocm.SelectChar;
    export.FilterServiceProvider.UserId = import.FilterServiceProvider.UserId;
    export.FilterPreviousServiceProvider.UserId =
      import.FilterPreviousServiceProvider.UserId;
    export.FilterDocument.Name = import.FilterDocument.Name;
    export.FilterPreviousDocument.Name = import.FilterPreviousDocument.Name;
    MoveOutgoingDocument(import.FilterOutgoingDocument,
      export.FilterOutgoingDocument);
    MoveOutgoingDocument(import.FilterPreviousOutgoingDocument,
      export.FilterPreviousOutgoingDocument);
    export.FilterCase.Number = import.FilterCase.Number;
    export.FilterPreviousCase.Number = import.FilterPreviousCase.Number;
    export.FilterCsePerson.Number = import.FilterCsePerson.Number;
    export.FilterPreviousCsePerson.Number =
      import.FilterPreviousCsePerson.Number;
    export.FilterAll.Flag = import.FilterAll.Flag;
    export.FilterPreviousAll.Flag = import.FilterPreviousAll.Flag;

    if (!IsEmpty(export.FilterCase.Number))
    {
      UseCabZeroFillNumber2();
    }

    if (!IsEmpty(export.FilterCsePerson.Number))
    {
      UseCabZeroFillNumber1();
    }

    // mjr
    // ----------------------------------------
    // 08/25/1998
    // Added selection count to this loop
    // -----------------------------------------------------
    local.SelCount.Count = 0;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      export.Export1.Update.Gdocument.Name = import.Import1.Item.Gdocument.Name;
      MoveOutgoingDocument(import.Import1.Item.GoutgoingDocument,
        export.Export1.Update.GoutgoingDocument);
      MoveInfrastructure(import.Import1.Item.Ginfrastructure,
        export.Export1.Update.Ginfrastructure);

      switch(AsChar(import.Import1.Item.Gcommon.SelectChar))
      {
        case 'S':
          ++local.SelCount.Count;

          break;
        case '*':
          export.Export1.Update.Gcommon.SelectChar = "";

          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }
    }

    import.Import1.CheckIndex();
    MoveStandard(import.Scrolling, export.Scrolling);

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.GexportHidden.SystemGeneratedIdentifier =
        import.HiddenPageKeys.Item.GimportHidden.SystemGeneratedIdentifier;
    }

    import.HiddenPageKeys.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // ---------------------------------------------
    // 01/29/1999
    // Validate 'Show all' indicator.
    // ----------------------------------------------------------
    switch(AsChar(export.FilterAll.Flag))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        export.FilterAll.Flag = "N";

        break;
      default:
        var field1 = GetField(export.FilterAll, "flag");

        field1.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        return;
    }

    // mjr
    // ---------------------------------------------
    // 07/17/1999
    // Validate SVPL Prompt character
    // ----------------------------------------------------------
    switch(AsChar(export.PromptSvpl.SelectChar))
    {
      case 'S':
        if (!Equal(global.Command, "LIST"))
        {
          var field2 = GetField(export.PromptSvpl, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }

        break;
      case '+':
        break;
      case ' ':
        break;
      default:
        var field1 = GetField(export.PromptSvpl, "selectChar");

        field1.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
    }

    // ----------------------------------------
    // Validate DOCM Prompt character
    // ----------------------------------------
    switch(AsChar(export.PromptDocm.SelectChar))
    {
      case 'S':
        if (!Equal(global.Command, "LIST"))
        {
          var field2 = GetField(export.PromptDocm, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }

        break;
      case '+':
        break;
      case ' ':
        break;
      default:
        var field1 = GetField(export.PromptDocm, "selectChar");

        field1.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // The user requested a next tran action
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();

      // mjr
      // -----------------------------------------------
      // 07/16/1999
      // Remove auto display on next tran into screen
      // ------------------------------------------------------------
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        if (IsEmpty(export.FilterDocument.Name) && IsEmpty
          (export.FilterServiceProvider.UserId))
        {
          var field1 = GetField(export.FilterDocument, "name");

          field1.Error = true;

          var field2 = GetField(export.FilterServiceProvider, "userId");

          field2.Error = true;

          ExitState = "OE0000_AT_LEAST_ONE_MAND_FIELD";

          return;
        }
      }
      else
      {
        // -------------------
        // continue processing
        // -------------------
      }

      if (!Equal(export.FilterOutgoingDocument.LastUpdatdTstamp,
        export.FilterPreviousOutgoingDocument.LastUpdatdTstamp) || !
        Equal(export.FilterDocument.Name, export.FilterPreviousDocument.Name) ||
        AsChar(export.FilterOutgoingDocument.PrintSucessfulIndicator) != AsChar
        (export.FilterPreviousOutgoingDocument.PrintSucessfulIndicator) || AsChar
        (export.FilterAll.Flag) != AsChar(export.FilterPreviousAll.Flag) || !
        Equal(export.FilterCase.Number, export.FilterPreviousCase.Number) || !
        Equal(export.FilterCsePerson.Number,
        export.FilterPreviousCsePerson.Number) || !
        Equal(export.FilterServiceProvider.UserId,
        export.FilterPreviousServiceProvider.UserId))
      {
        if (!Equal(export.FilterOutgoingDocument.LastUpdatdTstamp,
          export.FilterPreviousOutgoingDocument.LastUpdatdTstamp) && Lt
          (local.Null1.Timestamp, export.FilterOutgoingDocument.LastUpdatdTstamp))
          
        {
          // mjr
          // ------------------------------------------------------
          // 04/07/1999
          // If user types in a date, they want to see documents for that date.
          // Example:  User types in 03261999.  IEF translates that to
          // 1999-03-26-00.00.00.000000, which excludes any documents printed on
          // 03261999.  So add 1 day (1999-03-27-00.00.00.000000) then subtract
          // 1 microsecond (1999-03-26-23.59.59.999999).  This should give all
          // documents printed on or before 03261999.
          // -------------------------------------------------------------------
          export.FilterOutgoingDocument.LastUpdatdTstamp =
            AddMicroseconds(AddDays(
              export.FilterOutgoingDocument.LastUpdatdTstamp, 1), -1);
        }

        // mjr
        // --------------------------------------------------
        // 07/16/1999
        // At least one of the four major filters must be populated
        // 	* User Id
        // 	* Document Name
        // 	* Case Number
        // 	* Person Number
        // If none are populated, default User Id
        // ---------------------------------------------------------------
        if (IsEmpty(export.FilterServiceProvider.UserId) && IsEmpty
          (export.FilterDocument.Name) && IsEmpty(export.FilterCase.Number) && IsEmpty
          (export.FilterCsePerson.Number))
        {
          export.FilterServiceProvider.UserId =
            local.CurrentServiceProvider.UserId;
        }

        export.FilterPreviousServiceProvider.UserId =
          export.FilterServiceProvider.UserId;
        MoveOutgoingDocument(export.FilterOutgoingDocument,
          export.FilterPreviousOutgoingDocument);
        export.FilterPreviousDocument.Name = export.FilterDocument.Name;
        export.FilterPreviousAll.Flag = export.FilterAll.Flag;
        export.FilterPreviousCase.Number = export.FilterCase.Number;

        // ------------------------------------------------------------------------------------
        // 06/02/2008 MFan CQ415/PR186740  Added the following MOVE to fix the 
        // paging problem.
        // ------------------------------------------------------------------------------------
        export.FilterPreviousCsePerson.Number = export.FilterCsePerson.Number;
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "PRINT") || Equal(global.Command, "DDOC"))
    {
      if (local.SelCount.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
          }
        }

        export.Export1.CheckIndex();
      }
      else if (local.SelCount.Count < 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Export1.CheckIndex();
      }
    }

    // mjr
    // -----------------------------------------------------
    //         M A I N   C A S E   O F   C O M M A N D
    // --------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DDOC":
        // mjr-----> Flow to DDOC to display previously printed document
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            export.Selected.SystemGeneratedIdentifier =
              export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier;
            global.Command = "DISPLAY";
            ExitState = "ECO_LNK_TO_DDOC";

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "LIST":
        if (IsEmpty(export.PromptSvpl.SelectChar) && IsEmpty
          (export.PromptDocm.SelectChar))
        {
          var field1 = GetField(export.PromptSvpl, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (!IsEmpty(export.PromptDocm.SelectChar) && !
          IsEmpty(export.PromptSvpl.SelectChar))
        {
          var field1 = GetField(export.PromptSvpl, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptDocm, "selectChar");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
        }
        else if (!IsEmpty(export.PromptDocm.SelectChar))
        {
          export.PromptDocm.SelectChar = "";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";
        }
        else
        {
          export.PromptSvpl.SelectChar = "";
          ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";
        }

        return;
      case "RETLINK":
        if (!IsEmpty(import.FromPrompt.UserId))
        {
          export.FilterServiceProvider.UserId = import.FromPrompt.UserId;
          local.SpPrintWorkSet.FirstName = import.FromPrompt.FirstName;
          local.SpPrintWorkSet.MidInitial = import.FromPrompt.MiddleInitial;
          local.SpPrintWorkSet.LastName = import.FromPrompt.LastName;
          export.ServiceProviderName.FormattedName = UseSpDocFormatName();

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            export.Export1.Update.Gcommon.SelectChar = "*";

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "RETDOCM":
        if (!IsEmpty(import.ReturnDoc.Name))
        {
          export.FilterDocument.Name = import.ReturnDoc.Name;
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "CANCEL":
        if (local.SelCount.Count < 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          }

          export.Export1.CheckIndex();
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
          {
            continue;
          }

          if (AsChar(export.Export1.Item.GoutgoingDocument.
            PrintSucessfulIndicator) == 'Y' || AsChar
            (export.Export1.Item.GoutgoingDocument.PrintSucessfulIndicator) == 'C'
            || AsChar
            (export.Export1.Item.GoutgoingDocument.PrintSucessfulIndicator) == 'E'
            )
          {
            var field1 =
              GetField(export.Export1.Item.GoutgoingDocument,
              "printSucessfulIndicator");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";

            return;
          }
          else
          {
            UseSpDocCancelOutgoingDoc();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "*";
            export.Export1.Update.GoutgoingDocument.PrintSucessfulIndicator =
              "C";

            // mjr
            // -----------------------------------------------------
            // 06/29/2000
            // WR# 173 Seg B - Update CSE_Person when user cancels one of the
            // Family Violence Removal Letters
            // ------------------------------------------------------------------
            if (Equal(export.Export1.Item.Gdocument.Name, "APFVLTR"))
            {
              if (ReadCsePerson2())
              {
                try
                {
                  UpdateCsePerson();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      break;
                    case ErrorCode.PermittedValueViolation:
                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            else if (Equal(export.Export1.Item.Gdocument.Name, "ARFVLTR"))
            {
              if (ReadInfrastructure2())
              {
                if (ReadCsePerson1())
                {
                  local.Temp.Date = entities.CsePerson.FvLetterSentDate;

                  try
                  {
                    UpdateCsePerson();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        break;
                      case ErrorCode.PermittedValueViolation:
                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  foreach(var item in ReadCsePerson4())
                  {
                    try
                    {
                      UpdateCsePerson();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          break;
                        case ErrorCode.PermittedValueViolation:
                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
              }
            }
            else if (Equal(export.Export1.Item.Gdocument.Name, "CHFVLTR"))
            {
              if (ReadInfrastructure2())
              {
                if (ReadFieldValue())
                {
                  if (ReadCsePerson3())
                  {
                    try
                    {
                      UpdateCsePerson();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          break;
                        case ErrorCode.PermittedValueViolation:
                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
              }
            }
            else
            {
            }
          }
        }

        export.Export1.CheckIndex();

        break;
      case "NEXT":
        if (export.Scrolling.PageNumber == Export.HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (export.HiddenPageKeys.Item.GexportHidden.
          SystemGeneratedIdentifier <= 0)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Scrolling.PageNumber;

        break;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Scrolling.PageNumber;

        break;
      case "DISPLAY":
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHidden.SystemGeneratedIdentifier =
          0;

        break;
      case "PRINT":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
          {
            continue;
          }

          if (ReadFieldFieldValue())
          {
            if (Equal(entities.FieldValue.Value, "Y"))
            {
              ExitState = "LE0000_EIWO_DOC_CAN_NOT_PRINT";

              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              return;
            }
          }

          // mjr
          // --------------------------------------
          // 02/12/2001
          // WR# 187 - Auto IWO
          // Changed following IF statement to check for M.
          // Used to check for B, which is no longer used
          // M represents a document that will be emailed.
          // DDOC will give a message that informs the user,
          // and doesn't allow the download.
          // ---------------------------------------------------
          if (AsChar(export.Export1.Item.GoutgoingDocument.
            PrintSucessfulIndicator) == 'Y' || AsChar
            (export.Export1.Item.GoutgoingDocument.PrintSucessfulIndicator) == 'M'
            )
          {
            // mjr-----> Needs to flow to DDOC to re-Print document
            export.Selected.SystemGeneratedIdentifier =
              export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier;

            // mjr-----> Command is already set to Print
            ExitState = "ECO_LNK_TO_DDOC";
          }
          else
          {
            // mjr-----> Needs to flow to DKEY to try to gather data for Print
            export.Standard.NextTransaction = "DKEY";
            export.Hidden.InfrastructureId =
              export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier;

            // mjr
            // -----------------------------------------
            // 06/29/1999
            // Store filter values in next_tran for return from Print
            // ------------------------------------------------------
            export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
            export.HiddenPageKeys.CheckSize();

            export.Hidden.MiscText1 = "ODCM:" + export
              .FilterServiceProvider.UserId + export
              .FilterOutgoingDocument.PrintSucessfulIndicator + export
              .FilterAll.Flag + export.FilterDocument.Name + export
              .FilterCase.Number + NumberToString
              (export.HiddenPageKeys.Item.GexportHidden.
                SystemGeneratedIdentifier, 15);
            local.PrintProcess.Flag = "Y";
            local.PrintProcess.Command = "PRINT";
            UseScCabNextTranPut2();
          }

          return;
        }

        export.Export1.CheckIndex();

        break;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 10/09/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        local.Position.Count = Find(export.Hidden.MiscText1, "ODCM:");

        if (local.Position.Count == 1)
        {
          export.FilterServiceProvider.UserId =
            Substring(export.Hidden.MiscText1, 6, 8);
          export.FilterOutgoingDocument.PrintSucessfulIndicator =
            Substring(export.Hidden.MiscText1, 14, 1);
          export.FilterAll.Flag = Substring(export.Hidden.MiscText1, 15, 1);
          export.FilterDocument.Name =
            Substring(export.Hidden.MiscText1, 16, 8);
          export.FilterCase.Number = Substring(export.Hidden.MiscText1, 24, 10);
          export.HiddenPageKeys.Update.GexportHidden.SystemGeneratedIdentifier =
            (int)StringToNumber(Substring(export.Hidden.MiscText1, 50, 34, 15));
            
        }

        global.Command = "DISPLAY";

        break;
      case "":
        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // -----------------------------------------------------------
    // Display
    // -----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.Export1.Count = 0;
      export.ServiceProviderName.FormattedName = "";

      if (!IsEmpty(export.FilterServiceProvider.UserId))
      {
        if (ReadServiceProvider())
        {
          local.SpPrintWorkSet.FirstName = entities.ServiceProvider.FirstName;
          local.SpPrintWorkSet.MidInitial =
            entities.ServiceProvider.MiddleInitial;
          local.SpPrintWorkSet.LastName = entities.ServiceProvider.LastName;
          export.ServiceProviderName.FormattedName = UseSpDocFormatName();
        }
        else
        {
          if (ReadProgramProcessingInfo())
          {
            export.ServiceProviderName.FormattedName =
              entities.ProgramProcessingInfo.Description ?? Spaces(33);
          }

          if (IsEmpty(export.ServiceProviderName.FormattedName))
          {
            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }
        }
      }

      export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      local.PageStartKeyInfrastructure.SystemGeneratedIdentifier =
        export.HiddenPageKeys.Item.GexportHidden.SystemGeneratedIdentifier;

      // mjr
      // ----------------------------------------------
      // 04/06/1999
      // Determine the outgoing_document last_updated_tstamp related
      // to the local_page_start_key infrastructure.
      // -----------------------------------------------------------
      if (local.PageStartKeyInfrastructure.SystemGeneratedIdentifier > 0)
      {
        if (ReadOutgoingDocument())
        {
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp =
            entities.OutgoingDocument.LastUpdatdTstamp;
        }
        else
        {
          ExitState = "OUTGOING_DOCUMENT_NF";

          return;
        }
      }
      else if (Lt(local.Null1.Timestamp,
        export.FilterOutgoingDocument.LastUpdatdTstamp))
      {
        local.PageStartKeyOutgoingDocument.LastUpdatdTstamp =
          export.FilterOutgoingDocument.LastUpdatdTstamp;
      }
      else
      {
        local.PageStartKeyOutgoingDocument.LastUpdatdTstamp = Now();
      }

      // mjr
      // -------------------------------------------------
      // 07/16/1999
      // Separate READ EACHs for performance gains.
      // --------------------------------------------------------------
      if (!IsEmpty(export.FilterServiceProvider.UserId))
      {
        local.FilterMinimumServiceProvider.UserId =
          export.FilterServiceProvider.UserId;
        local.FilterMaximumServiceProvider.UserId =
          export.FilterServiceProvider.UserId;
      }
      else
      {
        local.FilterMaximumServiceProvider.UserId = "99999999";
      }

      if (!IsEmpty(export.FilterDocument.Name))
      {
        local.FilterMinimumDocument.Name = export.FilterDocument.Name;
        local.FilterMaximumDocument.Name = export.FilterDocument.Name;
      }
      else
      {
        local.FilterMaximumDocument.Name = "99999999";
      }

      if (!IsEmpty(export.FilterCase.Number))
      {
        local.FilterMinimumCase.Number = export.FilterCase.Number;
        local.FilterMaximumCase.Number = export.FilterCase.Number;
      }
      else
      {
        local.FilterMaximumCase.Number = "9999999999";
      }

      if (!IsEmpty(export.FilterCsePerson.Number))
      {
        local.FilterMinimumCsePerson.Number = export.FilterCsePerson.Number;
        local.FilterMaximumCsePerson.Number = export.FilterCsePerson.Number;
      }
      else
      {
        local.FilterMaximumCsePerson.Number = "9999999999";
      }

      export.Export1.Index = -1;

      if (!IsEmpty(export.FilterCase.Number) || !
        IsEmpty(export.FilterCsePerson.Number))
      {
        if (AsChar(export.FilterAll.Flag) == 'N')
        {
          if (!IsEmpty(export.FilterOutgoingDocument.PrintSucessfulIndicator))
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show one specific Print Successful Indicator value
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ
            // EACH, so that the program can get correct records to display and
            // do it efficiently.
            // -------------------------------------------------------------------------------------------------
            if (!IsEmpty(export.FilterCase.Number))
            {
              // 09/21/10 JHuss CQ22087 Emergency fix to improve performance of 
              // screen.
              // Adding "or 0 = 1" forces index CKI02777 to be used instead of 
              // the less
              // efficient index CKI03777.
              foreach(var item in ReadOutgoingDocumentInfrastructureDocument1())
              {
                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
            else
            {
              // -------------------------------------------------------------------------------------------------
              // 06/02/2008 MFan CQ415/PR186740  Made change to the following 
              // READ EACH, so that the program can get correct records to
              // display and do it efficiently.
              // -------------------------------------------------------------------------------------------------
              foreach(var item in ReadOutgoingDocumentInfrastructureDocument4())
              {
                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
          }
          else
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show all Print Successful Indicator values except C
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ
            // EACH, so that the program can get correct records to display and
            // do it efficiently.
            // -------------------------------------------------------------------------------------------------
            if (!IsEmpty(export.FilterCase.Number))
            {
              // 09/21/10 JHuss CQ22087 Emergency fix to improve performance of 
              // screen.
              // Adding "or 0 = 1" forces index CKI02777 to be used instead of 
              // the less
              // efficient index CKI03777.
              foreach(var item in ReadOutgoingDocumentInfrastructureDocument2())
              {
                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
            else
            {
              // ----------------------------------------------------------------------------------
              // 06/02/2008 MFan CQ415/PR186740  Made change to the following 
              // READ EACH, so that the program can get correct records to
              // display and do it efficiently.
              // -------------------------------------------------------------------------------------------------
              foreach(var item in ReadOutgoingDocumentInfrastructureDocument5())
              {
                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
          }
        }
        else
        {
          // mjr
          // -------------------------------------------
          // 07/16/1999
          // Show all Print Successful Indicator values
          // --------------------------------------------------------
          // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ 
          // EACH, so that the program can get correct records to display and do
          // it efficiently.
          // -------------------------------------------------------------------------------------------------
          if (!IsEmpty(export.FilterCase.Number))
          {
            // 09/21/10 JHuss CQ22087 Emergency fix to improve performance of 
            // screen.
            // Adding "or 0 = 1" forces index CKI02777 to be used instead of the
            // less
            // efficient index CKI03777.
            foreach(var item in ReadOutgoingDocumentInfrastructureDocument3())
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();

              if (export.Export1.Index >= Export.ExportGroup.Capacity)
              {
                break;
              }

              export.Export1.Update.Gcommon.SelectChar = "";
              export.Export1.Update.Gdocument.Name = entities.Document.Name;
              MoveOutgoingDocument(entities.OutgoingDocument,
                export.Export1.Update.GoutgoingDocument);
              MoveInfrastructure(entities.Infrastructure,
                export.Export1.Update.Ginfrastructure);
            }
          }
          else
          {
            // ------------------------------------------------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ
            // EACH, so that the program can get correct records to display and
            // do it efficiently.
            // -------------------------------------------------------------------------------------------------
            foreach(var item in ReadOutgoingDocumentInfrastructureDocument6())
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();

              if (export.Export1.Index >= Export.ExportGroup.Capacity)
              {
                break;
              }

              export.Export1.Update.Gcommon.SelectChar = "";
              export.Export1.Update.Gdocument.Name = entities.Document.Name;
              MoveOutgoingDocument(entities.OutgoingDocument,
                export.Export1.Update.GoutgoingDocument);
              MoveInfrastructure(entities.Infrastructure,
                export.Export1.Update.Ginfrastructure);
            }
          }
        }
      }
      else if (!IsEmpty(export.FilterServiceProvider.UserId))
      {
        if (AsChar(export.FilterAll.Flag) == 'N')
        {
          if (!IsEmpty(export.FilterOutgoingDocument.PrintSucessfulIndicator))
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show one specific Print Successful Indicator value
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ
            // EACH, so that the program can
            // get correct records to display and do it efficiently.
            // ----------------------------------------------------------------
            // 09/27/10 JHuss CQ22190 Emergency fix to improve performance of 
            // screen.
            // Adding "or 0 = 1" forces index CKI08777 to be used instead of the
            // less
            // efficient index CKI03419.
            foreach(var item in ReadOutgoingDocumentInfrastructureDocument8())
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();

              if (export.Export1.Index >= Export.ExportGroup.Capacity)
              {
                break;
              }

              export.Export1.Update.Gcommon.SelectChar = "";
              export.Export1.Update.Gdocument.Name = entities.Document.Name;
              MoveOutgoingDocument(entities.OutgoingDocument,
                export.Export1.Update.GoutgoingDocument);
              MoveInfrastructure(entities.Infrastructure,
                export.Export1.Update.Ginfrastructure);
            }
          }
          else
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show all Print Successful Indicator values except C
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ
            // EACH, so that the program can
            // get correct records to display and do it efficiently.
            // ----------------------------------------------------------------
            // 09/27/10 JHuss CQ22190 Emergency fix to improve performance of 
            // screen.
            // Adding "or 0 = 1" forces index CKI08777 to be used instead of the
            // less
            // efficient index CKI03419.
            foreach(var item in ReadOutgoingDocumentInfrastructureDocument7())
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();

              if (export.Export1.Index >= Export.ExportGroup.Capacity)
              {
                break;
              }

              export.Export1.Update.Gcommon.SelectChar = "";
              export.Export1.Update.Gdocument.Name = entities.Document.Name;
              MoveOutgoingDocument(entities.OutgoingDocument,
                export.Export1.Update.GoutgoingDocument);
              MoveInfrastructure(entities.Infrastructure,
                export.Export1.Update.Ginfrastructure);
            }
          }
        }
        else
        {
          // mjr
          // -------------------------------------------
          // 07/16/1999
          // Show all Print Successful Indicator values
          // --------------------------------------------------------
          // 06/02/2008 MFan CQ415/PR186740  Made change to the following READ 
          // EACH, so that the program can
          // get correct records to display and do it efficiently.
          // ----------------------------------------------------------------
          // 09/27/10 JHuss CQ22190 Emergency fix to improve performance of 
          // screen.
          // Adding "or 0 = 1" forces index CKI08777 to be used instead of the 
          // less
          // efficient index CKI03419.
          foreach(var item in ReadOutgoingDocumentInfrastructureDocument9())
          {
            ++export.Export1.Index;
            export.Export1.CheckSize();

            if (export.Export1.Index >= Export.ExportGroup.Capacity)
            {
              break;
            }

            export.Export1.Update.Gcommon.SelectChar = "";
            export.Export1.Update.Gdocument.Name = entities.Document.Name;
            MoveOutgoingDocument(entities.OutgoingDocument,
              export.Export1.Update.GoutgoingDocument);
            MoveInfrastructure(entities.Infrastructure,
              export.Export1.Update.Ginfrastructure);
          }
        }
      }
      else
      {
        // ------Document Name is entered,
        // 
        // ------
        // 
        // ------User id, case, and person are NOT entered.-------
        // *Change by Carl galka on 01/10/2000.
        // 
        // *Per problem report 83411 ODCM Performance issue when  only entering
        // 
        // *Document Name.
        // 
        // *Now, we will only display one month of data at a time from the date
        // entered.
        // ------------------------------------------------------------------------------------------------------------------
        // 09/19/2008   M Fan   CQ415/PR186740 -- Seg B (Phase ll )
        //                      changed the way to calculate local filter 
        // minimum outgoing
        //                      document last updatd tstamp.
        // ------------------------------------------------------------------------------------------------------------------
        if (Equal(export.FilterOutgoingDocument.LastUpdatdTstamp,
          local.Null1.Timestamp))
        {
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp =
            AddMonths(local.CurrentDateWorkArea.Timestamp, -1);
        }
        else if (Lt(export.FilterOutgoingDocument.LastUpdatdTstamp,
          new DateTime(1, 2, 2)))
        {
          // 10/30/2009	JHuss	CQ 12686
          // If the date entered is less than 2/2/0001, subtracting 1 month 
          // causes an abend.
          // Explicity set it to 1/1/0001.
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp =
            new DateTime(1, 1, 1);
        }
        else
        {
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp =
            AddMicroseconds(AddDays(
              AddMonths(export.FilterOutgoingDocument.LastUpdatdTstamp, -1), -
            1), 1);
        }

        if (AsChar(export.FilterAll.Flag) == 'N')
        {
          if (!IsEmpty(export.FilterOutgoingDocument.PrintSucessfulIndicator))
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show one specific Print Successful Indicator value
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made the following changes, so 
            // that the program can get
            // correct records to display and do it efficiently:
            // 1. Decomposed the origional READ EACH with 3 tables (outgoing 
            // document, infrastructure and
            // document) joined into a READ EACH with 2 tables (outgoing 
            // document and document) joined and a
            // READ for infrastructure.
            // 2. SORTED outgoing doucuments by last updated timestamp in 
            // descending order and created
            // timestamp in ascending order. Since created timestamp is unique, 
            // it will maintain the same
            // sequence for those records with duplicated last updated timestamp
            // whenever they are sorted.
            // ----------------------------------------------------------------------------------------------
            foreach(var item in ReadOutgoingDocumentDocument2())
            {
              if (ReadInfrastructure1())
              {
                if (Equal(entities.OutgoingDocument.LastUpdatdTstamp,
                  local.PageStartKeyOutgoingDocument.LastUpdatdTstamp) && entities
                  .Infrastructure.SystemGeneratedIdentifier != local
                  .PageStartKeyInfrastructure.SystemGeneratedIdentifier && export
                  .Export1.Index == -1)
                {
                  continue;
                }

                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
          }
          else
          {
            // mjr
            // -------------------------------------------
            // 07/16/1999
            // Show all Print Successful Indicator values except C
            // --------------------------------------------------------
            // 06/02/2008 MFan CQ415/PR186740  Made the following changes, so 
            // that the program can get
            // correct records to display and do it efficiently:
            // 1. Decomposed the origional READ EACH with 3 tables (outgoing 
            // document, infrastructure and
            // document) joined into a READ EACH with 2 tables (outgoing 
            // document and document) joined and a
            // READ for infrastructure.
            // 2. SORTED outgoing doucuments by last updated timestamp in 
            // descending order and created
            // timestamp in ascending order. Since created timestamp is unique, 
            // it will maintain the same
            // sequence for those records with duplicated last updated timestamp
            // whenever they are sorted.
            // -------------------------------------------------------------------------------------------------
            foreach(var item in ReadOutgoingDocumentDocument1())
            {
              if (ReadInfrastructure1())
              {
                if (Equal(entities.OutgoingDocument.LastUpdatdTstamp,
                  local.PageStartKeyOutgoingDocument.LastUpdatdTstamp) && entities
                  .Infrastructure.SystemGeneratedIdentifier != local
                  .PageStartKeyInfrastructure.SystemGeneratedIdentifier && export
                  .Export1.Index == -1)
                {
                  continue;
                }

                ++export.Export1.Index;
                export.Export1.CheckSize();

                if (export.Export1.Index >= Export.ExportGroup.Capacity)
                {
                  break;
                }

                export.Export1.Update.Gcommon.SelectChar = "";
                export.Export1.Update.Gdocument.Name = entities.Document.Name;
                MoveOutgoingDocument(entities.OutgoingDocument,
                  export.Export1.Update.GoutgoingDocument);
                MoveInfrastructure(entities.Infrastructure,
                  export.Export1.Update.Ginfrastructure);
              }
            }
          }
        }
        else
        {
          // mjr
          // -------------------------------------------
          // 07/16/1999
          // Show all Print Successful Indicator values
          // --------------------------------------------------------
          // 06/02/2008 MFan CQ415/PR186740  Made the following changes, so that
          // the program can get correct records to display and do it
          // efficiently:
          // 1. Decomposed the origional READ EACH with 3 tables (outgoing 
          // document, infrastructure and document) joined into a READ EACH with
          // 2 tables (outgoing document and document) joined and a READ for
          // infrastructure.
          // 2. SORTED outgoing doucuments by last updated timestamp in 
          // descending order and created timestamp in ascending order. Since
          // created timestamp is unique, it will maintain the same sequence for
          // those records with duplicated last updated timestamp whenever they
          // are sorted.
          // -------------------------------------------------------------------------------------------------
          foreach(var item in ReadOutgoingDocumentDocument3())
          {
            if (ReadInfrastructure1())
            {
              if (Equal(entities.OutgoingDocument.LastUpdatdTstamp,
                local.PageStartKeyOutgoingDocument.LastUpdatdTstamp) && entities
                .Infrastructure.SystemGeneratedIdentifier != local
                .PageStartKeyInfrastructure.SystemGeneratedIdentifier && export
                .Export1.Index == -1)
              {
                continue;
              }

              ++export.Export1.Index;
              export.Export1.CheckSize();

              if (export.Export1.Index >= Export.ExportGroup.Capacity)
              {
                break;
              }

              export.Export1.Update.Gcommon.SelectChar = "";
              export.Export1.Update.Gdocument.Name = entities.Document.Name;
              MoveOutgoingDocument(entities.OutgoingDocument,
                export.Export1.Update.GoutgoingDocument);
              MoveInfrastructure(entities.Infrastructure,
                export.Export1.Update.Ginfrastructure);
            }
          }
        }
      }

      if (export.Export1.IsEmpty)
      {
        local.PageStartKeyInfrastructure.SystemGeneratedIdentifier = 0;

        if (export.Scrolling.PageNumber == 1)
        {
          export.Scrolling.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }
      }

      if (export.Export1.IsFull && export.Export1.Index >= Export
        .ExportGroup.Capacity)
      {
        local.PageStartKeyInfrastructure.SystemGeneratedIdentifier =
          entities.Infrastructure.SystemGeneratedIdentifier;

        if (export.Scrolling.PageNumber > 1)
        {
          export.Scrolling.ScrollingMessage = "MORE - +";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE   +";
        }
      }
      else
      {
        local.PageStartKeyInfrastructure.SystemGeneratedIdentifier = 0;

        if (export.Scrolling.PageNumber <= 1)
        {
          export.Scrolling.ScrollingMessage = "MORE";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE -";
        }
      }

      if (export.HiddenPageKeys.Index + 1 == Export
        .HiddenPageKeysGroup.Capacity)
      {
        ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
        export.Scrolling.ScrollingMessage = "MORE -";
      }
      else
      {
        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHidden.SystemGeneratedIdentifier =
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr
        // ----------------------------------------------
        // 10/09/1998
        // Added check for an exitstate returned from Print
        // -----------------------------------------------------------
        local.Position.Count = Find(export.Hidden.MiscText2, "PRINT:");

        if (local.Position.Count <= 0)
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.PrintRetCode.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.PrintRetCode.Text50;
        }
      }

      if (local.Position.Count <= 0)
      {
      }
      else
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier == export
            .Hidden.InfrastructureId.GetValueOrDefault())
          {
            export.Export1.Update.Gcommon.SelectChar = "*";

            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventDetailName = source.EventDetailName;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOutgoingDocument(OutgoingDocument source,
    OutgoingDocument target)
  {
    target.PrintSucessfulIndicator = source.PrintSucessfulIndicator;
    target.LastUpdatdTstamp = source.LastUpdatdTstamp;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.FilterCsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.FilterCsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.FilterCase.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.FilterCase.Number = useImport.Case1.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
    useImport.NextTranInfo.Assign(export.Hidden);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpDocCancelOutgoingDoc()
  {
    var useImport = new SpDocCancelOutgoingDoc.Import();
    var useExport = new SpDocCancelOutgoingDoc.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier;

    Call(SpDocCancelOutgoingDoc.Execute, useImport, useExport);
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.PrintRetCode.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.PrintRetCode.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fvLtrSentDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetString(
          command, "casNumber", entities.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
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
        db.SetInt32(
          command, "systemGeneratedI",
          export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "value", entities.FieldValue.Value ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fvLtrSentDt", local.Temp.Date.GetValueOrDefault());
        db.SetString(
          command, "casNumber", entities.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadFieldFieldValue()
  {
    entities.FieldValue.Populated = false;
    entities.Field.Populated = false;

    return Read("ReadFieldFieldValue",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 0);
        entities.FieldValue.Value = db.GetNullableString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
        entities.Field.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(
          command, "infIdentifier",
          entities.Infrastructure.SystemGeneratedIdentifier);
        db.SetString(command, "docName", export.Export1.Item.Gdocument.Name);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadInfrastructure1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 1);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.UserId = db.GetString(reader, 4);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Export1.Item.Ginfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 1);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.UserId = db.GetString(reader, 4);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentDocument1()
  {
    entities.Document.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentDocument1",
      (db, command) =>
      {
        db.SetNullableString(command, "docName", export.FilterDocument.Name);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp1",
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp2",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Document.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentDocument2()
  {
    entities.Document.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentDocument2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docName", local.FilterMinimumDocument.Name);
        db.SetString(
          command, "prntSucessfulInd",
          export.FilterOutgoingDocument.PrintSucessfulIndicator);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp1",
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp2",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Document.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentDocument3()
  {
    entities.Document.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentDocument3",
      (db, command) =>
      {
        db.SetNullableString(command, "docName", export.FilterDocument.Name);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp1",
          local.FilterMinimumOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp2",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Document.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument1()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument1",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetNullableString(
          command, "caseNumber", local.FilterMinimumCase.Number);
        db.SetString(command, "number1", local.FilterMinimumCsePerson.Number);
        db.SetString(command, "number2", local.FilterMaximumCsePerson.Number);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetString(
          command, "prntSucessfulInd",
          export.FilterOutgoingDocument.PrintSucessfulIndicator);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument2()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument2",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetNullableString(
          command, "caseNumber", local.FilterMinimumCase.Number);
        db.SetString(command, "number1", local.FilterMinimumCsePerson.Number);
        db.SetString(command, "number2", local.FilterMaximumCsePerson.Number);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument3()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument3",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetNullableString(
          command, "caseNumber", local.FilterMinimumCase.Number);
        db.SetString(command, "number1", local.FilterMinimumCsePerson.Number);
        db.SetString(command, "number2", local.FilterMaximumCsePerson.Number);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument4()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument4",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetNullableString(
          command, "csePersonNum", local.FilterMinimumCsePerson.Number);
        db.SetString(
          command, "prntSucessfulInd",
          export.FilterOutgoingDocument.PrintSucessfulIndicator);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument5()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument5",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetNullableString(
          command, "csePersonNum", local.FilterMinimumCsePerson.Number);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument6()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument6",
      (db, command) =>
      {
        db.SetString(
          command, "userId1", local.FilterMinimumServiceProvider.UserId);
        db.SetString(
          command, "userId2", local.FilterMaximumServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetString(command, "number1", local.FilterMinimumCsePerson.Number);
        db.SetString(command, "number2", local.FilterMaximumCsePerson.Number);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument7()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument7",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument8()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument8",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetString(
          command, "prntSucessfulInd",
          export.FilterOutgoingDocument.PrintSucessfulIndicator);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructureDocument9()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructureDocument9",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
        db.SetString(command, "name1", local.FilterMinimumDocument.Name);
        db.SetString(command, "name2", local.FilterMaximumDocument.Name);
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.PageStartKeyOutgoingDocument.LastUpdatdTstamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.PageStartKeyInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 4);
        entities.Document.Name = db.GetString(reader, 4);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 9);
        entities.Infrastructure.UserId = db.GetString(reader, 10);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", export.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.Description =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = local.CurrentDateWorkArea.Timestamp;
    var lastUpdatedBy = local.CurrentServiceProvider.UserId;
    var fvLetterSentDate = local.Null1.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "fvLtrSentDt", fvLetterSentDate);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FvLetterSentDate = fvLetterSentDate;
    entities.CsePerson.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of Gdocument.
      /// </summary>
      [JsonPropertyName("gdocument")]
      public Document Gdocument
      {
        get => gdocument ??= new();
        set => gdocument = value;
      }

      /// <summary>
      /// A value of GoutgoingDocument.
      /// </summary>
      [JsonPropertyName("goutgoingDocument")]
      public OutgoingDocument GoutgoingDocument
      {
        get => goutgoingDocument ??= new();
        set => goutgoingDocument = value;
      }

      /// <summary>
      /// A value of Ginfrastructure.
      /// </summary>
      [JsonPropertyName("ginfrastructure")]
      public Infrastructure Ginfrastructure
      {
        get => ginfrastructure ??= new();
        set => ginfrastructure = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common gcommon;
      private Document gdocument;
      private OutgoingDocument goutgoingDocument;
      private Infrastructure ginfrastructure;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHidden.
      /// </summary>
      [JsonPropertyName("gimportHidden")]
      public Infrastructure GimportHidden
      {
        get => gimportHidden ??= new();
        set => gimportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private Infrastructure gimportHidden;
    }

    /// <summary>
    /// A value of ReturnDoc.
    /// </summary>
    [JsonPropertyName("returnDoc")]
    public Document ReturnDoc
    {
      get => returnDoc ??= new();
      set => returnDoc = value;
    }

    /// <summary>
    /// A value of PromtDocm.
    /// </summary>
    [JsonPropertyName("promtDocm")]
    public Common PromtDocm
    {
      get => promtDocm ??= new();
      set => promtDocm = value;
    }

    /// <summary>
    /// A value of PromptSvpl.
    /// </summary>
    [JsonPropertyName("promptSvpl")]
    public Common PromptSvpl
    {
      get => promptSvpl ??= new();
      set => promptSvpl = value;
    }

    /// <summary>
    /// A value of FromPrompt.
    /// </summary>
    [JsonPropertyName("fromPrompt")]
    public ServiceProvider FromPrompt
    {
      get => fromPrompt ??= new();
      set => fromPrompt = value;
    }

    /// <summary>
    /// A value of FilterPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("filterPreviousCsePerson")]
    public CsePerson FilterPreviousCsePerson
    {
      get => filterPreviousCsePerson ??= new();
      set => filterPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of FilterCsePerson.
    /// </summary>
    [JsonPropertyName("filterCsePerson")]
    public CsePerson FilterCsePerson
    {
      get => filterCsePerson ??= new();
      set => filterCsePerson = value;
    }

    /// <summary>
    /// A value of FilterPreviousCase.
    /// </summary>
    [JsonPropertyName("filterPreviousCase")]
    public Case1 FilterPreviousCase
    {
      get => filterPreviousCase ??= new();
      set => filterPreviousCase = value;
    }

    /// <summary>
    /// A value of FilterCase.
    /// </summary>
    [JsonPropertyName("filterCase")]
    public Case1 FilterCase
    {
      get => filterCase ??= new();
      set => filterCase = value;
    }

    /// <summary>
    /// A value of FilterPreviousAll.
    /// </summary>
    [JsonPropertyName("filterPreviousAll")]
    public Common FilterPreviousAll
    {
      get => filterPreviousAll ??= new();
      set => filterPreviousAll = value;
    }

    /// <summary>
    /// A value of FilterAll.
    /// </summary>
    [JsonPropertyName("filterAll")]
    public Common FilterAll
    {
      get => filterAll ??= new();
      set => filterAll = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterPreviousServiceProvider.
    /// </summary>
    [JsonPropertyName("filterPreviousServiceProvider")]
    public ServiceProvider FilterPreviousServiceProvider
    {
      get => filterPreviousServiceProvider ??= new();
      set => filterPreviousServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOutgoingDocument.
    /// </summary>
    [JsonPropertyName("filterOutgoingDocument")]
    public OutgoingDocument FilterOutgoingDocument
    {
      get => filterOutgoingDocument ??= new();
      set => filterOutgoingDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousOutgoingDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousOutgoingDocument")]
    public OutgoingDocument FilterPreviousOutgoingDocument
    {
      get => filterPreviousOutgoingDocument ??= new();
      set => filterPreviousOutgoingDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousDocument")]
    public Document FilterPreviousDocument
    {
      get => filterPreviousDocument ??= new();
      set => filterPreviousDocument = value;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of ServiceProviderName.
    /// </summary>
    [JsonPropertyName("serviceProviderName")]
    public CsePersonsWorkSet ServiceProviderName
    {
      get => serviceProviderName ??= new();
      set => serviceProviderName = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Document returnDoc;
    private Common promtDocm;
    private Common promptSvpl;
    private ServiceProvider fromPrompt;
    private CsePerson filterPreviousCsePerson;
    private CsePerson filterCsePerson;
    private Case1 filterPreviousCase;
    private Case1 filterCase;
    private Common filterPreviousAll;
    private Common filterAll;
    private ServiceProvider filterServiceProvider;
    private ServiceProvider filterPreviousServiceProvider;
    private OutgoingDocument filterOutgoingDocument;
    private OutgoingDocument filterPreviousOutgoingDocument;
    private Document filterPreviousDocument;
    private Document filterDocument;
    private CsePersonsWorkSet serviceProviderName;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private NextTranInfo hidden;
    private Standard scrolling;
    private Standard standard;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of Gdocument.
      /// </summary>
      [JsonPropertyName("gdocument")]
      public Document Gdocument
      {
        get => gdocument ??= new();
        set => gdocument = value;
      }

      /// <summary>
      /// A value of GoutgoingDocument.
      /// </summary>
      [JsonPropertyName("goutgoingDocument")]
      public OutgoingDocument GoutgoingDocument
      {
        get => goutgoingDocument ??= new();
        set => goutgoingDocument = value;
      }

      /// <summary>
      /// A value of Ginfrastructure.
      /// </summary>
      [JsonPropertyName("ginfrastructure")]
      public Infrastructure Ginfrastructure
      {
        get => ginfrastructure ??= new();
        set => ginfrastructure = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common gcommon;
      private Document gdocument;
      private OutgoingDocument goutgoingDocument;
      private Infrastructure ginfrastructure;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public Infrastructure GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private Infrastructure gexportHidden;
    }

    /// <summary>
    /// A value of PromptDocm.
    /// </summary>
    [JsonPropertyName("promptDocm")]
    public Common PromptDocm
    {
      get => promptDocm ??= new();
      set => promptDocm = value;
    }

    /// <summary>
    /// A value of PromptSvpl.
    /// </summary>
    [JsonPropertyName("promptSvpl")]
    public Common PromptSvpl
    {
      get => promptSvpl ??= new();
      set => promptSvpl = value;
    }

    /// <summary>
    /// A value of FilterPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("filterPreviousCsePerson")]
    public CsePerson FilterPreviousCsePerson
    {
      get => filterPreviousCsePerson ??= new();
      set => filterPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of FilterCsePerson.
    /// </summary>
    [JsonPropertyName("filterCsePerson")]
    public CsePerson FilterCsePerson
    {
      get => filterCsePerson ??= new();
      set => filterCsePerson = value;
    }

    /// <summary>
    /// A value of FilterPreviousCase.
    /// </summary>
    [JsonPropertyName("filterPreviousCase")]
    public Case1 FilterPreviousCase
    {
      get => filterPreviousCase ??= new();
      set => filterPreviousCase = value;
    }

    /// <summary>
    /// A value of FilterCase.
    /// </summary>
    [JsonPropertyName("filterCase")]
    public Case1 FilterCase
    {
      get => filterCase ??= new();
      set => filterCase = value;
    }

    /// <summary>
    /// A value of FilterPreviousAll.
    /// </summary>
    [JsonPropertyName("filterPreviousAll")]
    public Common FilterPreviousAll
    {
      get => filterPreviousAll ??= new();
      set => filterPreviousAll = value;
    }

    /// <summary>
    /// A value of FilterAll.
    /// </summary>
    [JsonPropertyName("filterAll")]
    public Common FilterAll
    {
      get => filterAll ??= new();
      set => filterAll = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterPreviousServiceProvider.
    /// </summary>
    [JsonPropertyName("filterPreviousServiceProvider")]
    public ServiceProvider FilterPreviousServiceProvider
    {
      get => filterPreviousServiceProvider ??= new();
      set => filterPreviousServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOutgoingDocument.
    /// </summary>
    [JsonPropertyName("filterOutgoingDocument")]
    public OutgoingDocument FilterOutgoingDocument
    {
      get => filterOutgoingDocument ??= new();
      set => filterOutgoingDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousOutgoingDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousOutgoingDocument")]
    public OutgoingDocument FilterPreviousOutgoingDocument
    {
      get => filterPreviousOutgoingDocument ??= new();
      set => filterPreviousOutgoingDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousDocument")]
    public Document FilterPreviousDocument
    {
      get => filterPreviousDocument ??= new();
      set => filterPreviousDocument = value;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Infrastructure Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of ServiceProviderName.
    /// </summary>
    [JsonPropertyName("serviceProviderName")]
    public CsePersonsWorkSet ServiceProviderName
    {
      get => serviceProviderName ??= new();
      set => serviceProviderName = value;
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common promptDocm;
    private Common promptSvpl;
    private CsePerson filterPreviousCsePerson;
    private CsePerson filterCsePerson;
    private Case1 filterPreviousCase;
    private Case1 filterCase;
    private Common filterPreviousAll;
    private Common filterAll;
    private ServiceProvider filterServiceProvider;
    private ServiceProvider filterPreviousServiceProvider;
    private OutgoingDocument filterOutgoingDocument;
    private OutgoingDocument filterPreviousOutgoingDocument;
    private Document filterPreviousDocument;
    private Document filterDocument;
    private Infrastructure selected;
    private CsePersonsWorkSet serviceProviderName;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private NextTranInfo hidden;
    private Standard scrolling;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ConstantOne.
    /// </summary>
    [JsonPropertyName("constantOne")]
    public Common ConstantOne
    {
      get => constantOne ??= new();
      set => constantOne = value;
    }

    /// <summary>
    /// A value of ConstantZero.
    /// </summary>
    [JsonPropertyName("constantZero")]
    public Common ConstantZero
    {
      get => constantZero ??= new();
      set => constantZero = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of CurrentServiceProvider.
    /// </summary>
    [JsonPropertyName("currentServiceProvider")]
    public ServiceProvider CurrentServiceProvider
    {
      get => currentServiceProvider ??= new();
      set => currentServiceProvider = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of FilterMinimumOutgoingDocument.
    /// </summary>
    [JsonPropertyName("filterMinimumOutgoingDocument")]
    public OutgoingDocument FilterMinimumOutgoingDocument
    {
      get => filterMinimumOutgoingDocument ??= new();
      set => filterMinimumOutgoingDocument = value;
    }

    /// <summary>
    /// A value of FilterMaximumCsePerson.
    /// </summary>
    [JsonPropertyName("filterMaximumCsePerson")]
    public CsePerson FilterMaximumCsePerson
    {
      get => filterMaximumCsePerson ??= new();
      set => filterMaximumCsePerson = value;
    }

    /// <summary>
    /// A value of FilterMaximumCase.
    /// </summary>
    [JsonPropertyName("filterMaximumCase")]
    public Case1 FilterMaximumCase
    {
      get => filterMaximumCase ??= new();
      set => filterMaximumCase = value;
    }

    /// <summary>
    /// A value of FilterMaximumServiceProvider.
    /// </summary>
    [JsonPropertyName("filterMaximumServiceProvider")]
    public ServiceProvider FilterMaximumServiceProvider
    {
      get => filterMaximumServiceProvider ??= new();
      set => filterMaximumServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterMaximumDocument.
    /// </summary>
    [JsonPropertyName("filterMaximumDocument")]
    public Document FilterMaximumDocument
    {
      get => filterMaximumDocument ??= new();
      set => filterMaximumDocument = value;
    }

    /// <summary>
    /// A value of FilterMinimumCsePerson.
    /// </summary>
    [JsonPropertyName("filterMinimumCsePerson")]
    public CsePerson FilterMinimumCsePerson
    {
      get => filterMinimumCsePerson ??= new();
      set => filterMinimumCsePerson = value;
    }

    /// <summary>
    /// A value of FilterMinimumCase.
    /// </summary>
    [JsonPropertyName("filterMinimumCase")]
    public Case1 FilterMinimumCase
    {
      get => filterMinimumCase ??= new();
      set => filterMinimumCase = value;
    }

    /// <summary>
    /// A value of FilterMinimumServiceProvider.
    /// </summary>
    [JsonPropertyName("filterMinimumServiceProvider")]
    public ServiceProvider FilterMinimumServiceProvider
    {
      get => filterMinimumServiceProvider ??= new();
      set => filterMinimumServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterMinimumDocument.
    /// </summary>
    [JsonPropertyName("filterMinimumDocument")]
    public Document FilterMinimumDocument
    {
      get => filterMinimumDocument ??= new();
      set => filterMinimumDocument = value;
    }

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
    }

    /// <summary>
    /// A value of PageStartKeyInfrastructure.
    /// </summary>
    [JsonPropertyName("pageStartKeyInfrastructure")]
    public Infrastructure PageStartKeyInfrastructure
    {
      get => pageStartKeyInfrastructure ??= new();
      set => pageStartKeyInfrastructure = value;
    }

    /// <summary>
    /// A value of PageStartKeyOutgoingDocument.
    /// </summary>
    [JsonPropertyName("pageStartKeyOutgoingDocument")]
    public OutgoingDocument PageStartKeyOutgoingDocument
    {
      get => pageStartKeyOutgoingDocument ??= new();
      set => pageStartKeyOutgoingDocument = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
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
    /// A value of PrintRetCode.
    /// </summary>
    [JsonPropertyName("printRetCode")]
    public WorkArea PrintRetCode
    {
      get => printRetCode ??= new();
      set => printRetCode = value;
    }

    private Common constantOne;
    private Common constantZero;
    private DateWorkArea temp;
    private ServiceProvider currentServiceProvider;
    private DateWorkArea currentDateWorkArea;
    private OutgoingDocument filterMinimumOutgoingDocument;
    private CsePerson filterMaximumCsePerson;
    private Case1 filterMaximumCase;
    private ServiceProvider filterMaximumServiceProvider;
    private Document filterMaximumDocument;
    private CsePerson filterMinimumCsePerson;
    private Case1 filterMinimumCase;
    private ServiceProvider filterMinimumServiceProvider;
    private Document filterMinimumDocument;
    private Common printProcess;
    private Infrastructure pageStartKeyInfrastructure;
    private OutgoingDocument pageStartKeyOutgoingDocument;
    private SpPrintWorkSet spPrintWorkSet;
    private DateWorkArea null1;
    private Common selCount;
    private Common position;
    private WorkArea printRetCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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

    private FieldValue fieldValue;
    private Field field;
    private DocumentField documentField;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private Document document;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private ServiceProvider serviceProvider;
  }
#endregion
}
