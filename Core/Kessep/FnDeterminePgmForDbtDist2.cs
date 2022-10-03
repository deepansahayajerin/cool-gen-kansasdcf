// Program: FN_DETERMINE_PGM_FOR_DBT_DIST_2, ID: 374492885, model: 746.
// Short name: SWE02401
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PGM_FOR_DBT_DIST_2.
/// </summary>
[Serializable]
public partial class FnDeterminePgmForDbtDist2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PGM_FOR_DBT_DIST_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeterminePgmForDbtDist2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeterminePgmForDbtDist2.
  /// </summary>
  public FnDeterminePgmForDbtDist2(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ------------  ------------	
    // -------------------------------------------------------------------------------------
    // ??/??/??  ????????????			Initial Code.
    // 11/01/08  GVandy	CQ#4387		Distribution 2009
    // 			CQ#468		Correct derivation when NA-CA changes to an insterstate 
    // program
    // 			CQ#500 		MAI program should result in derivation the same as the NAI 
    // program
    // 08/08/14  GVandy	CQ45444		Correct initial program derivation where the 
    // program
    // 					effective date is after the collection date but prior
    // 					to the debt due date.  This happens when money
    // 					applies to Future support and then retro processing
    // 					occurs due to a program change effective between
    // 					the collection date and the debt due date.
    // 03/09/16  GVandy	CQ42608		Do not pass "ura exists for type only" values 
    // to
    // 					cab fn_determine_ura_for_supp_prsn.  These values
    // 					cause the cab to not net URA amounts across AE cases.
    // 					There was a situation were a positive URA amount
    // 					existed on one AE case and a negative URA balance
    // 					existed on another AE case which netted to zero.
    // 					The result was an AF program derivation with no URA
    // 					to offset which caused the cash receipt detail
    // 					to suspend with NODEBTPERS reason code.
    // 					
    // -------------------------------------------------------------------------------------------------------------------------------
    // : All Debt Details with a Due Date/Covered Period Start Date prior to an 
    // associated AF program which became effective prior to 10-1-1997, stay AF-
    // PA.
    local.AfPaAllArearsPriorTo.Date = new DateTime(1997, 10, 1);
    local.Distribution2009Start.Date = new DateTime(2009, 10, 1);

    // : Set up the Year/Month to use for determining if the Debt Detail is 
    // Current or Arrears.
    if (Equal(import.Collection.Date, local.NullDateWorkArea.Date))
    {
      local.Collection.Date = Now().Date;
    }
    else
    {
      local.Collection.Date = import.Collection.Date;
    }

    local.Collection.YearMonth = UseCabGetYearMonthFromDate1();

    // : Accruing verses Non-Accruing types of Obligations are processed 
    // differently.  Determine the type and continue processing.
    if (AsChar(import.ObligationType.Classification) == AsChar
      (import.HardcodedAccruingClass.Classification))
    {
      local.DebtDue.Date = import.DebtDetail.DueDt;
    }
    else
    {
      local.DebtDue.Date = import.DebtDetail.CoveredPrdStartDt;
    }

    local.DebtDue.YearMonth = UseCabGetYearMonthFromDate2();
    export.Program.Assign(local.NullProgram);
    export.DprProgram.ProgramState = local.NullDprProgram.ProgramState;

    if (IsEmpty(import.DebtDetail.PreconversionProgramCode))
    {
      for(import.PgmHistDtl.Index = 0; import.PgmHistDtl.Index < import
        .PgmHistDtl.Count; ++import.PgmHistDtl.Index)
      {
        // 08/08/14  GVandy  CQ45444  Correct initial program derivation where 
        // the program effective date
        // is after the collection date but prior to the debt due date.  This 
        // happens when money applies to
        // Future support and then retro processing occurs due to a program 
        // change effective between the
        // collection date and the debt due date.
        if (!Lt(local.DebtDue.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && !
          Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate,
          local.DebtDue.Date) && (
            import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
          .HardcodedAf.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedAfi.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFci.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNaProgram.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedMai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNf.SystemGeneratedIdentifier))
        {
          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            goto Test1;
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier != import
              .HardcodedFc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNc.SystemGeneratedIdentifier)
          {
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier != import
              .HardcodedAf.SystemGeneratedIdentifier && export
              .Program.SystemGeneratedIdentifier != import
              .HardcodedFc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNf.SystemGeneratedIdentifier)
          {
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier != import
              .HardcodedAf.SystemGeneratedIdentifier && export
              .Program.SystemGeneratedIdentifier != import
              .HardcodedFc.SystemGeneratedIdentifier && export
              .Program.SystemGeneratedIdentifier != import
              .HardcodedNc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }

          if (import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
            .HardcodedMai.SystemGeneratedIdentifier)
          {
            // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
            // the same as the NAI program
            export.Program.Assign(import.HardcodedNai);
          }
          else
          {
            export.Program.Assign(import.PgmHistDtl.Item.PgmHistDtlProgram);
          }

          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNaProgram.SystemGeneratedIdentifier)
          {
            export.DprProgram.ProgramState =
              import.HardcodedNaDprProgram.ProgramState;
          }
          else
          {
            export.DprProgram.ProgramState = local.NullDprProgram.ProgramState;
          }
        }
      }
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedAf.Code))
    {
      export.Program.Assign(import.HardcodedAf);
      export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedAfi.Code))
    {
      export.Program.Assign(import.HardcodedAfi);
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedFc.Code))
    {
      export.Program.Assign(import.HardcodedFc);
      export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedFci.Code))
    {
      export.Program.Assign(import.HardcodedFci);
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedNaProgram.Code))
    {
      export.Program.Assign(import.HardcodedNaProgram);
      export.DprProgram.ProgramState =
        import.HardcodedNaDprProgram.ProgramState;
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedNai.Code))
    {
      export.Program.Assign(import.HardcodedNai);
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedNc.Code))
    {
      export.Program.Assign(import.HardcodedNc);
    }
    else if (Equal(import.DebtDetail.PreconversionProgramCode,
      import.HardcodedNf.Code))
    {
      export.Program.Assign(import.HardcodedNf);
    }
    else
    {
      // : Fall thru to the logic for handling no person program found.
    }

Test1:

    // : If not program has been found, set a default program as the baseline 
    // for the matrix processing.
    if (export.Program.SystemGeneratedIdentifier == 0)
    {
      for(import.PgmHistDtl.Index = 0; import.PgmHistDtl.Index < import
        .PgmHistDtl.Count; ++import.PgmHistDtl.Index)
      {
        if (!Lt(local.DebtDue.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && !
          Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate,
          local.DebtDue.Date))
        {
          export.Program.Assign(import.HardcodedNaProgram);
          export.DprProgram.ProgramState =
            import.HardcodedNaDprProgram.ProgramState;

          goto Test2;
        }
      }

      if (AsChar(import.Obligation.OrderTypeCode) == 'K')
      {
        if (local.DebtDue.YearMonth >= local.Collection.YearMonth)
        {
          export.Program.Assign(import.HardcodedNaProgram);
          export.DprProgram.ProgramState =
            import.HardcodedNaDprProgram.ProgramState;
        }
        else
        {
          export.Program.Assign(import.HardcodedAf);
          export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
        }
      }
      else if (local.DebtDue.YearMonth >= local.Collection.YearMonth)
      {
        export.Program.Assign(import.HardcodedNai);
        export.DprProgram.ProgramState = local.NullDprProgram.ProgramState;
      }
      else
      {
        export.Program.Assign(import.HardcodedAfi);
        export.DprProgram.ProgramState = local.NullDprProgram.ProgramState;
      }
    }

Test2:

    // -- This finds the "To New Program" on the PRWORA distribution matrix.
    // -- 11/01/08  GVandy  CQ#4387  Distribution 2009   Added the IF statement 
    // below...
    // This will increase performance by not executing the for each if the debt 
    // due date is after the distribution 2009 start date.
    // The for each would not find any person programs in this scenario so no 
    // need to waste the time spinning through the group view.
    if (Lt(local.DebtDue.Date, local.Distribution2009Start.Date))
    {
      // -- Initialize the max AF date.
      local.MaxAfDiscDt.Date = local.NullDateWorkArea.Date;

      // : Process the Person/Program Matrix to determine current program.
      for(import.PgmHistDtl.Index = 0; import.PgmHistDtl.Index < import
        .PgmHistDtl.Count; ++import.PgmHistDtl.Index)
      {
        // -- 11/01/08  GVandy  CQ#4387  Distribution 2009 -
        // Added check for program effective date less than distribution 2009 
        // start date.
        if (Lt(local.DebtDue.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && !
          Lt(local.Collection.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && Lt
          (import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate,
          local.Distribution2009Start.Date) && (
            import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
          .HardcodedAf.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedAfi.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFci.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNaProgram.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedMai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNf.SystemGeneratedIdentifier))
        {
          // 11/01/08  GVandy  CQ#468  Added the following statements to track 
          // the maximum AF discontinue date.
          //      Will need this following this FOR EACH when determining 
          // whether to change an AF-TA derivation to NA-CA due AF closure.
          if (import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            if (Lt(local.MaxAfDiscDt.Date,
              import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate))
            {
              local.MaxAfDiscDt.Date =
                import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate;
            }
          }

          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            if (Equal(export.DprProgram.ProgramState, "TA"))
            {
              if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                SystemGeneratedIdentifier != import
                .HardcodedAf.SystemGeneratedIdentifier)
              {
                // 11/01/08  GVandy  CQ#468  If derivation is currently AF-TA 
                // and the new program is not AF then the derivation should
                // change to NA-CA.
                export.Program.Assign(import.HardcodedNaProgram);
                export.DprProgram.ProgramState =
                  import.HardcodedCa.ProgramState;
              }
            }
            else
            {
            }

            continue;
          }

          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNaProgram.SystemGeneratedIdentifier)
          {
            switch(TrimEnd(export.DprProgram.ProgramState))
            {
              case "NA":
                if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedAf.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedAf);

                  if (!Lt(local.AfPaAllArearsPriorTo.Date,
                    import.PgmHistDtl.Item.PgmHistDtlPersonProgram.
                      EffectiveDate))
                  {
                    export.DprProgram.ProgramState =
                      import.HardcodedPa.ProgramState;
                  }
                  else
                  {
                    export.DprProgram.ProgramState =
                      import.HardcodedTa.ProgramState;
                  }
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedAfi.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedAfi);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedFci.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedFci);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedNai.SystemGeneratedIdentifier || import
                  .PgmHistDtl.Item.PgmHistDtlProgram.
                    SystemGeneratedIdentifier == import
                  .HardcodedMai.SystemGeneratedIdentifier)
                {
                  // 11/01/08  GVandy  CQ#500 MAI program should result in 
                  // derivation the same as the NAI program
                  export.Program.Assign(import.HardcodedNai);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }

                break;
              case "CA":
                if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedAf.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedAf);

                  if (!Lt(local.AfPaAllArearsPriorTo.Date,
                    import.PgmHistDtl.Item.PgmHistDtlPersonProgram.
                      EffectiveDate))
                  {
                    export.DprProgram.ProgramState =
                      import.HardcodedPa.ProgramState;
                  }
                  else
                  {
                    export.DprProgram.ProgramState =
                      import.HardcodedTa.ProgramState;
                  }
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedAfi.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedAfi);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedFci.SystemGeneratedIdentifier)
                {
                  export.Program.Assign(import.HardcodedFci);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }
                else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
                  SystemGeneratedIdentifier == import
                  .HardcodedNai.SystemGeneratedIdentifier || import
                  .PgmHistDtl.Item.PgmHistDtlProgram.
                    SystemGeneratedIdentifier == import
                  .HardcodedMai.SystemGeneratedIdentifier)
                {
                  // 11/01/08  GVandy  CQ#500 MAI program should result in 
                  // derivation the same as the NAI program
                  export.Program.Assign(import.HardcodedNai);
                  export.DprProgram.ProgramState =
                    local.NullDprProgram.ProgramState;
                }

                break;
              default:
                break;
            }

            continue;
          }

          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNai.SystemGeneratedIdentifier)
          {
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAf.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAf);

              if (!Lt(local.AfPaAllArearsPriorTo.Date,
                import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate))
              {
                export.DprProgram.ProgramState =
                  import.HardcodedPa.ProgramState;
              }
              else
              {
                export.DprProgram.ProgramState =
                  import.HardcodedTa.ProgramState;
              }
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedNaProgram.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedNaProgram);
              export.DprProgram.ProgramState =
                import.HardcodedNaDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAfi.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAfi);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedFci.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedFci);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }

            continue;
          }
        }
      }
    }

    // -----------------------------------------------------------------------------------------------------------
    // 11/01/08  GVandy  CQ#4387  Distribution 2009
    //                    Distribution 2009 matrix processing.
    // -----------------------------------------------------------------------------------------------------------
    // -- This finds the "To New Program" on the distribution 2009 matrix.
    if (!Lt(import.Collection.Date, local.Distribution2009Start.Date))
    {
      // 		~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
      // 		~	     DISTRIBUTION  2009 	    ~
      // 		~	Person Program Arrears Matrix	    ~
      // 		~	    Effective 10/01/2009	    ~
      // 		~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
      // Arrears
      // were:      To new program:
      //            +---------+---------+---------+---------+---------+---------
      // +---------+---------+
      //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI    
      // |  NF     |  NC     |
      // +=========++=========+=========+=========+=========+=========+=========
      // +=========+=========+
      // |  AF-PA  ||  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  
      // |  AF-PA  |  AF-PA  |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  FC-PA  ||  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  
      // |  FC-PA  |  FC-PA  |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  NA-NA  ||  NA-NA  |  NA-NA  |  NA-NA  |  AFI    |  FCI    |  NAI    
      // |  NA-NA  |  NA-NA  |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  AFI    ||  AFI    |  AFI    |  AFI    |  AFI    |  AFI    |  AFI    
      // |  AFI    |  AFI    |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  FCI    ||  FCI    |  FCI    |  FCI    |  FCI    |  FCI    |  FCI    
      // |  FCI    |  FCI    |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  NAI    ||  NAI    |  NAI    |  NA-NA  |  AFI    |  FCI    |  NAI    
      // |  NAI    |  NAI    |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  NC     ||  NC     |  NC     |  NC     |  NC     |  NC     |  NC     
      // |  NC     |  NC     |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  NF     ||  NF     |  NF     |  NF     |  NF     |  NF     |  NF     
      // |  NF     |  NF     |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  NA-CA  ||  NA-CA  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  NAI    
      // |  NA-CA  |  NA-CA  |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // |  AF-TA  ||  **N/A  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  NAI    
      // |  NA-CA  |  NA-CA  |
      // +---------++---------+---------+---------+---------+---------+---------
      // +---------+---------+
      // ** If the TAF program is active as of 10/1/2009, the AF-TA arrears will
      // remain AF-TA until
      //    the TAF program closes.  At that point, the arrears become NA-CA and
      // will not change,
      //    even if the TAF program becomes active again.
      for(import.PgmHistDtl.Index = 0; import.PgmHistDtl.Index < import
        .PgmHistDtl.Count; ++import.PgmHistDtl.Index)
      {
        // -- Find all person program records which became effective after 10/01
        // /2009.
        //    We will process these records using the new Distribution 2009 
        // matrix.
        if (Lt(local.DebtDue.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && !
          Lt(local.Collection.Date,
          import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate) && !
          Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate,
          local.Distribution2009Start.Date) && (
            import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
          .HardcodedAf.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedAfi.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedFci.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNaProgram.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedMai.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNc.SystemGeneratedIdentifier || import
          .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
          .HardcodedNf.SystemGeneratedIdentifier))
        {
          if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier && Equal
            (export.DprProgram.ProgramState, import.HardcodedPa.ProgramState))
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  AF-PA  ||  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  
            // AF-PA  |  AF-PA  |  AF-PA  |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains AF-PA 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedFc.SystemGeneratedIdentifier && Equal
            (export.DprProgram.ProgramState, import.HardcodedPa.ProgramState))
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  FC-PA  ||  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  
            // FC-PA  |  FC-PA  |  FC-PA  |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains FC-PA 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNaProgram.SystemGeneratedIdentifier && Equal
            (export.DprProgram.ProgramState,
            import.HardcodedNaDprProgram.ProgramState))
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  NA-NA  ||  NA-NA  |  NA-NA  |  NA-NA  |  AFI    |  FCI    |  
            // NAI    |  NA-NA  |  NA-NA  |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAfi.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAfi);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedFci.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedFci);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedNai.SystemGeneratedIdentifier || import
              .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
              .HardcodedMai.SystemGeneratedIdentifier)
            {
              // 11/01/08  GVandy  CQ#500 MAI program should result in 
              // derivation the same as the NAI program
              export.Program.Assign(import.HardcodedNai);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else
            {
              // -- No processing required.  The debt derivation remains NA-NA 
              // for all non interstate programs.
            }
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAfi.SystemGeneratedIdentifier)
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  AFI    ||  AFI    |  AFI    |  AFI    |  AFI    |  AFI    |  
            // AFI    |  AFI    |  AFI    |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains AFI 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedFci.SystemGeneratedIdentifier)
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  FCI    ||  FCI    |  FCI    |  FCI    |  FCI    |  FCI    |  
            // FCI    |  FCI    |  FCI    |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains FCI 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNai.SystemGeneratedIdentifier)
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  NAI    ||  NAI    |  NAI    |  NA-NA  |  AFI    |  FCI    |  
            // NAI    |  NAI    |  NAI    |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedNaProgram.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedNaProgram);
              export.DprProgram.ProgramState =
                import.HardcodedNaDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAfi.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAfi);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedFci.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedFci);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else
            {
              // -- No processing required.  The debt derivation remains NAI for
              // all other programs.
            }
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNc.SystemGeneratedIdentifier)
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  NC     ||  NC     |  NC     |  NC     |  NC     |  NC     |  
            // NC     |  NC     |  NC     |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains NC 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNf.SystemGeneratedIdentifier)
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  NF     ||  NF     |  NF     |  NF     |  NF     |  NF     |  
            // NF     |  NF     |  NF     |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // -- No processing required.  The debt derivation remains NF 
            // regardless of the new program.
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedNaProgram.SystemGeneratedIdentifier && Equal
            (export.DprProgram.ProgramState, import.HardcodedCa.ProgramState))
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  NA-CA  ||  NA-CA  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  
            // NAI    |  NA-CA  |  NA-CA  |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAfi.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAfi);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedFci.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedFci);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedNai.SystemGeneratedIdentifier || import
              .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
              .HardcodedMai.SystemGeneratedIdentifier)
            {
              // 11/01/08  GVandy  CQ#500 MAI program should result in 
              // derivation the same as the NAI program
              export.Program.Assign(import.HardcodedNai);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else
            {
              // -- No processing required.  The debt derivation remains NA-CA 
              // for all non interstate programs.
            }
          }
          else if (export.Program.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier && Equal
            (export.DprProgram.ProgramState, import.HardcodedTa.ProgramState))
          {
            // Arrears
            // were:      To new program:
            //            +---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  
            // NAI    |  NF     |  NC     |
            // +=========++=========+=========+=========+=========+=========+
            // =========+=========+=========+
            // |  AF-TA  ||  **N/A  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  
            // NAI    |  NA-CA  |  NA-CA  |
            // +---------++---------+---------+---------+---------+---------+
            // ---------+---------+---------+
            // ** If the TAF program is active as of 10/1/2009, the AF-TA 
            // arrears will remain AF-TA until
            //    the TAF program closes.  At that point, the arrears become NA-
            // CA and will not change,
            //    even if the TAF program becomes active again.
            if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAf.SystemGeneratedIdentifier)
            {
              // -- No processing required.  The debt derivation remains AF-TA 
              // unless the TAF program in effect on 10/01/2009 has closed.
              //    We'll check for the closure of that program following this 
              // FOR EACH if the derivation is still AF-TA at that time.
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedAfi.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedAfi);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedFci.SystemGeneratedIdentifier)
            {
              export.Program.Assign(import.HardcodedFci);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else if (import.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == import
              .HardcodedNai.SystemGeneratedIdentifier || import
              .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
              .HardcodedMai.SystemGeneratedIdentifier)
            {
              // 11/01/08  GVandy  CQ#500 MAI program should result in 
              // derivation the same as the NAI program
              export.Program.Assign(import.HardcodedNai);
              export.DprProgram.ProgramState =
                local.NullDprProgram.ProgramState;
            }
            else
            {
              // -- The debt derivation changes to NA-CA for all other programs.
              export.Program.Assign(import.HardcodedNaProgram);
              export.DprProgram.ProgramState = import.HardcodedCa.ProgramState;
            }
          }

          // 11/01/08  GVandy  CQ#468  Added the following statements to track 
          // the maximum AF discontinue date.
          //      Will need this following this FOR EACH when determining 
          // whether to change an AF-TA derivation to NA-CA due AF closure.
          if (import.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            if (Lt(local.MaxAfDiscDt.Date,
              import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate))
            {
              local.MaxAfDiscDt.Date =
                import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate;
            }
          }
        }
      }

      // --  If PRWORA derivation is AF-TA and a TAF program was active on 10/01
      // /2009 and remains active then the
      //     derivation stays AF-TA.  Otherwise, the derivation becomes NA-CA (
      // AF-CA for F type collections).
      if (export.Program.SystemGeneratedIdentifier == import
        .HardcodedAf.SystemGeneratedIdentifier && Equal
        (export.DprProgram.ProgramState, import.HardcodedTa.ProgramState))
      {
        for(import.PgmHistDtl.Index = 0; import.PgmHistDtl.Index < import
          .PgmHistDtl.Count; ++import.PgmHistDtl.Index)
        {
          // -- Added check for program effective date less than distribution 
          // 2009 start date and discontinue date greater than distribution 2009
          // start date.
          if (Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.EffectiveDate,
            local.Distribution2009Start.Date) && !
            Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate,
            local.Distribution2009Start.Date) && !
            Lt(import.PgmHistDtl.Item.PgmHistDtlPersonProgram.DiscontinueDate,
            import.Collection.Date) && import
            .PgmHistDtl.Item.PgmHistDtlProgram.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            // -- The AF program which was active on 10/01/2009 is still 
            // effective, so we leave the derivation at AF-TA.
            goto Test3;
          }
        }

        // -- If we get to this point then the AF program is no longer 
        // effective.  Change the derivation from AF-TA to NA-CA.
        export.Program.Assign(import.HardcodedNaProgram);
        export.DprProgram.ProgramState = import.HardcodedCa.ProgramState;
      }
    }

Test3:

    // *******************************************************************
    // Handle the situation where the program is derived as AF-TA and the
    // AF program has actually ended prior to the date of collection.
    // If this is the case the AF-TA should be NA-CA .
    // (11/01/2008 - Moved this logic from the PRWORA matrix processing
    // section to here because there might have been another AF timeframe
    // starting after 10/01/2009 which means that the derivation should
    // remain AF-TA...)
    // *******************************************************************
    if (export.Program.SystemGeneratedIdentifier == import
      .HardcodedAf.SystemGeneratedIdentifier && Equal
      (export.DprProgram.ProgramState, import.HardcodedTa.ProgramState))
    {
      // 11/01/08  GVandy  CQ#468 If derivation is currently AF-TA and the max 
      // AF closure date is less than the date of collection then the
      // derivation should change to NA-CA.
      if (Lt(local.MaxAfDiscDt.Date, import.Collection.Date))
      {
        export.Program.Assign(import.HardcodedNaProgram);
        export.DprProgram.ProgramState = import.HardcodedCa.ProgramState;
      }
    }

    // -- Override NA-CA derivation on F type collections to AF-CA.
    if (import.CollectionType.SequentialIdentifier == import
      .HardcodedFFedType.SequentialIdentifier && export
      .Program.SystemGeneratedIdentifier == import
      .HardcodedNaProgram.SystemGeneratedIdentifier && Equal
      (export.DprProgram.ProgramState, import.HardcodedCa.ProgramState))
    {
      export.Program.Assign(import.HardcodedAf);
      export.DprProgram.ProgramState = import.HardcodedCa.ProgramState;
    }

    // : Override "NA" Program for 71B Obligaiton Types & Set Program to "AF".
    if (import.ObligationType.SystemGeneratedIdentifier == import
      .Hardcoded718B.SystemGeneratedIdentifier && export
      .Program.SystemGeneratedIdentifier == import
      .HardcodedNaProgram.SystemGeneratedIdentifier)
    {
      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
      // It appears that this logic will never be executed since 718Bs will not 
      // derive an NA program.  This results from ONAC setting the
      // preconversion program code to AF when the 718Bs are created causing the
      // "default" program derivation to be AF-PA.  In
      // accordance with the program derivation matrix AF-PA does not change 
      // when any new program opens.
      // There are some 718Bs created during conversion with preconversion 
      // program codes of FC and NF.  The "default" derivation for
      // these will be FC-PA and NF respectively.  Again according to the 
      // program derivation matrix FC-PA and NF do not change when
      // any new programs open.
      // So, bottom line is that there should never be an NA derivation on a 
      // 718B.  The code was left in place "just in case"...
      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
      export.Program.Assign(import.HardcodedAf);
      export.DprProgram.ProgramState = import.HardcodedPa.ProgramState;
    }

    if (AsChar(import.Obligation.PrimarySecondaryCode) == AsChar
      (import.HardcodedSecondary.PrimarySecondaryCode))
    {
      return;
    }

    if (export.Program.SystemGeneratedIdentifier == import
      .HardcodedAf.SystemGeneratedIdentifier || export
      .Program.SystemGeneratedIdentifier == import
      .HardcodedFc.SystemGeneratedIdentifier)
    {
      if (import.ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMcType.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMjType.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMsType.SystemGeneratedIdentifier)
      {
        UseFnDetermineUraForSuppPrsn();

        if (local.UraMedicalAmount.TotalCurrency <= 0)
        {
          export.Program.Assign(import.HardcodedNaProgram);

          switch(TrimEnd(export.DprProgram.ProgramState))
          {
            case "PA":
              export.DprProgram.ProgramState = import.HardcodedUd.ProgramState;

              break;
            case "TA":
              export.DprProgram.ProgramState = import.HardcodedUp.ProgramState;

              break;
            case "CA":
              export.DprProgram.ProgramState = import.HardcodedUp.ProgramState;

              break;
            default:
              break;
          }
        }
      }
      else if (import.ObligationType.SystemGeneratedIdentifier == import
        .Hardcoded718B.SystemGeneratedIdentifier)
      {
        UseFnDetermineUraForSuppPrsn();

        if (local.UraAmount.TotalCurrency <= 0)
        {
          export.DprProgram.ProgramState = import.HardcodedUk.ProgramState;
        }
      }
      else
      {
        UseFnDetermineUraForSuppPrsn();

        if (local.UraAmount.TotalCurrency <= 0)
        {
          export.Program.Assign(import.HardcodedNaProgram);

          switch(TrimEnd(export.DprProgram.ProgramState))
          {
            case "PA":
              export.DprProgram.ProgramState = import.HardcodedUd.ProgramState;

              break;
            case "TA":
              export.DprProgram.ProgramState = import.HardcodedUp.ProgramState;

              break;
            case "CA":
              export.DprProgram.ProgramState = import.HardcodedUp.ProgramState;

              break;
            default:
              break;
          }
        }
      }
    }

    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // @@@
    // 
    // @@@
    // @@@
    // 
    // @@@
    // @@@   ORIGINAL CODE BELOW IN ITS ENTIRETY  11/04/2008            @@@
    // @@@
    // 
    // @@@
    // @@@
    // 
    // @@@
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
  }

  private static void MoveHhHist(Import.HhHistGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl);
  }

  private static void MoveHhHistDtl(Import.HhHistDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal(Import.LegalGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalGroup target)
  {
    target.LegalSuppPrsn1.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl);
  }

  private static void MoveLegalDtl(Import.LegalDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Collection.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.DebtDue.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseFnDetermineUraForSuppPrsn()
  {
    var useImport = new FnDetermineUraForSuppPrsn.Import();
    var useExport = new FnDetermineUraForSuppPrsn.Export();

    useImport.SuppPrsn.Number = import.SuppPrsn.Number;
    useImport.Collection.Date = import.Collection.Date;
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist);
    useImport.UraExistsForTypeOnly.Text1 = local.UraExistsForTypeOnly.Text1;

    Call(FnDetermineUraForSuppPrsn.Execute, useImport, useExport);

    local.UraAmount.TotalCurrency = useExport.UraAmount.TotalCurrency;
    local.UraMedicalAmount.TotalCurrency =
      useExport.UraMedicalAmount.TotalCurrency;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn")]
      public CsePerson LegalSuppPrsn
      {
        get => legalSuppPrsn ??= new();
        set => legalSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of SuppPrsn.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    public CsePerson SuppPrsn
    {
      get => suppPrsn ??= new();
      set => suppPrsn = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// Gets a value of PgmHistDtl.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
      PgmHistDtlGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHistDtl for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHistDtl")]
    [Computed]
    public IList<PgmHistDtlGroup> PgmHistDtl_Json
    {
      get => pgmHistDtl;
      set => PgmHistDtl.Assign(value);
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of Hardcoded718B.
    /// </summary>
    [JsonPropertyName("hardcoded718B")]
    public ObligationType Hardcoded718B
    {
      get => hardcoded718B ??= new();
      set => hardcoded718B = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNaProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaProgram")]
    public Program HardcodedNaProgram
    {
      get => hardcodedNaProgram ??= new();
      set => hardcodedNaProgram = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of HardcodedPa.
    /// </summary>
    [JsonPropertyName("hardcodedPa")]
    public DprProgram HardcodedPa
    {
      get => hardcodedPa ??= new();
      set => hardcodedPa = value;
    }

    /// <summary>
    /// A value of HardcodedTa.
    /// </summary>
    [JsonPropertyName("hardcodedTa")]
    public DprProgram HardcodedTa
    {
      get => hardcodedTa ??= new();
      set => hardcodedTa = value;
    }

    /// <summary>
    /// A value of HardcodedNaDprProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaDprProgram")]
    public DprProgram HardcodedNaDprProgram
    {
      get => hardcodedNaDprProgram ??= new();
      set => hardcodedNaDprProgram = value;
    }

    /// <summary>
    /// A value of HardcodedCa.
    /// </summary>
    [JsonPropertyName("hardcodedCa")]
    public DprProgram HardcodedCa
    {
      get => hardcodedCa ??= new();
      set => hardcodedCa = value;
    }

    /// <summary>
    /// A value of HardcodedUd.
    /// </summary>
    [JsonPropertyName("hardcodedUd")]
    public DprProgram HardcodedUd
    {
      get => hardcodedUd ??= new();
      set => hardcodedUd = value;
    }

    /// <summary>
    /// A value of HardcodedUp.
    /// </summary>
    [JsonPropertyName("hardcodedUp")]
    public DprProgram HardcodedUp
    {
      get => hardcodedUp ??= new();
      set => hardcodedUp = value;
    }

    /// <summary>
    /// A value of HardcodedUk.
    /// </summary>
    [JsonPropertyName("hardcodedUk")]
    public DprProgram HardcodedUk
    {
      get => hardcodedUk ??= new();
      set => hardcodedUk = value;
    }

    /// <summary>
    /// A value of HardcodedFFedType.
    /// </summary>
    [JsonPropertyName("hardcodedFFedType")]
    public CollectionType HardcodedFFedType
    {
      get => hardcodedFFedType ??= new();
      set => hardcodedFFedType = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    private CsePerson suppPrsn;
    private ObligationType obligationType;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private DateWorkArea collection;
    private CollectionType collectionType;
    private LegalAction legalAction;
    private Array<PgmHistDtlGroup> pgmHistDtl;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private ObligationType hardcodedAccruingClass;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private CollectionType hardcodedFFedType;
    private Obligation hardcodedSecondary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    private Program program;
    private DprProgram dprProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Distribution2009Start.
    /// </summary>
    [JsonPropertyName("distribution2009Start")]
    public DateWorkArea Distribution2009Start
    {
      get => distribution2009Start ??= new();
      set => distribution2009Start = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of NullProgram.
    /// </summary>
    [JsonPropertyName("nullProgram")]
    public Program NullProgram
    {
      get => nullProgram ??= new();
      set => nullProgram = value;
    }

    /// <summary>
    /// A value of NullDprProgram.
    /// </summary>
    [JsonPropertyName("nullDprProgram")]
    public DprProgram NullDprProgram
    {
      get => nullDprProgram ??= new();
      set => nullDprProgram = value;
    }

    /// <summary>
    /// A value of UraMedicalAmount.
    /// </summary>
    [JsonPropertyName("uraMedicalAmount")]
    public Common UraMedicalAmount
    {
      get => uraMedicalAmount ??= new();
      set => uraMedicalAmount = value;
    }

    /// <summary>
    /// A value of UraAmount.
    /// </summary>
    [JsonPropertyName("uraAmount")]
    public Common UraAmount
    {
      get => uraAmount ??= new();
      set => uraAmount = value;
    }

    /// <summary>
    /// A value of AfPaAllArearsPriorTo.
    /// </summary>
    [JsonPropertyName("afPaAllArearsPriorTo")]
    public DateWorkArea AfPaAllArearsPriorTo
    {
      get => afPaAllArearsPriorTo ??= new();
      set => afPaAllArearsPriorTo = value;
    }

    /// <summary>
    /// A value of UraExistsForTypeOnly.
    /// </summary>
    [JsonPropertyName("uraExistsForTypeOnly")]
    public TextWorkArea UraExistsForTypeOnly
    {
      get => uraExistsForTypeOnly ??= new();
      set => uraExistsForTypeOnly = value;
    }

    /// <summary>
    /// A value of MaxAfDiscDt.
    /// </summary>
    [JsonPropertyName("maxAfDiscDt")]
    public DateWorkArea MaxAfDiscDt
    {
      get => maxAfDiscDt ??= new();
      set => maxAfDiscDt = value;
    }

    private DateWorkArea distribution2009Start;
    private DateWorkArea collection;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea debtDue;
    private Program nullProgram;
    private DprProgram nullDprProgram;
    private Common uraMedicalAmount;
    private Common uraAmount;
    private DateWorkArea afPaAllArearsPriorTo;
    private TextWorkArea uraExistsForTypeOnly;
    private DateWorkArea maxAfDiscDt;
  }
#endregion
}
