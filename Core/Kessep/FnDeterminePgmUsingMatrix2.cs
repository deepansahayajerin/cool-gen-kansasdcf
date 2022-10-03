// Program: FN_DETERMINE_PGM_USING_MATRIX_2, ID: 374422482, model: 746.
// Short name: SWE01752
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PGM_USING_MATRIX_2.
/// </summary>
[Serializable]
public partial class FnDeterminePgmUsingMatrix2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PGM_USING_MATRIX_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeterminePgmUsingMatrix2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeterminePgmUsingMatrix2.
  /// </summary>
  public FnDeterminePgmUsingMatrix2(IContext context, Import import,
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
    // 05/22/00  Madhu Kumar			Initial Code.
    // 08/22/00  Madhu Kumar			JJA Enhancements
    // 11/01/08  GVandy	CQ#4387		Distribution 2009
    // 			CQ#468		Correct derivation when NA-CA changes to an insterstate 
    // program
    // 			CQ#500 		MAI program should result in derivation the same as the NAI 
    // program
    // -------------------------------------------------------------------------------------------------------------------------------
    // : All Debt Details with a Due Date/Covered Period Start Date
    // prior to an associated AF program which became effective
    // prior to 10-1-97 ,stay AF-PA.
    local.AfPaAllArearsPriorTo.Date = new DateTime(1997, 10, 1);
    local.Distribution2009Start.Date = new DateTime(2009, 10, 1);

    // : Set hardcoded values for Program.
    local.HardcodedAf.SystemGeneratedIdentifier = 2;
    local.HardcodedAfi.SystemGeneratedIdentifier = 14;
    local.HardcodedFc.SystemGeneratedIdentifier = 15;
    local.HardcodedFci.SystemGeneratedIdentifier = 16;
    local.HardcodedNa.SystemGeneratedIdentifier = 12;
    local.HardcodedNai.SystemGeneratedIdentifier = 18;
    local.HardcodedNc.SystemGeneratedIdentifier = 13;
    local.HardcodedNf.SystemGeneratedIdentifier = 3;
    local.HardcodedMai.SystemGeneratedIdentifier = 17;
    export.KeyOnly.SystemGeneratedIdentifier =
      import.KeyOnly.SystemGeneratedIdentifier;
    export.DprProgram.ProgramState = import.DprProgram.ProgramState;

    // : If no initial Program has been identified, then estabish a baseline 
    // now.
    if (export.KeyOnly.SystemGeneratedIdentifier == 0)
    {
      foreach(var item in ReadPersonProgramProgram3())
      {
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFc.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
          UseFnDetermineProgramState();

          goto Test1;
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNc.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedAf.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNf.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedAf.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedNc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }

        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedMai.SystemGeneratedIdentifier)
        {
          // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
          // the same as the NAI program
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNai.SystemGeneratedIdentifier;
        }
        else
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
        }
      }

      if (export.KeyOnly.SystemGeneratedIdentifier != 0)
      {
        UseFnDetermineProgramState();

        goto Test1;
      }

      // : Not a problem, determine the default program at the time of the due 
      // date.
      if (ReadPersonProgramProgram1())
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNa.SystemGeneratedIdentifier;
        UseFnDetermineProgramState();
      }
      else
      {
        if (AsChar(import.Obligation.OrderTypeCode) == 'K')
        {
          if (import.DebtDue.YearMonth >= import.Collection.YearMonth)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNa.SystemGeneratedIdentifier;
          }
          else
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedAf.SystemGeneratedIdentifier;
          }
        }
        else if (import.DebtDue.YearMonth >= import.Collection.YearMonth)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNai.SystemGeneratedIdentifier;
        }
        else
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedAfi.SystemGeneratedIdentifier;
        }

        UseFnDetermineProgramState();
      }
    }

Test1:

    // -- This finds the "To New Program" on the PRWORA distribution matrix.
    // -- 11/01/08  GVandy  CQ#4387  Distribution 2009  -  Added the IF 
    // statement below...
    // This will increase performance by not attempting the read each if the 
    // debt due date is after the distribution 2009 start date.
    // The read each would not find any person programs in this scenario so no 
    // need to waste the time opening the cursor.
    if (Lt(import.DebtDue.Date, local.Distribution2009Start.Date))
    {
      // -- Initialize the max AF date.
      local.MaxAfDiscDt.Date = local.NullDate.Date;

      // -- 11/01/08  GVandy  CQ#4387  Distribution 2009 -
      // Added check for program effective date less than distribution 2009 
      // start date in the READ EACH where predicate.
      foreach(var item in ReadPersonProgramProgram5())
      {
        // 11/01/08  GVandy  CQ#468  Added the following statements to track the
        // maximum AF discontinue date.
        // Will need this following this READ EACH when determining whether to 
        // change an AF-TA derivation to NA-CA due AF closure.
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          if (Lt(local.MaxAfDiscDt.Date, entities.Existing.DiscontinueDate))
          {
            local.MaxAfDiscDt.Date = entities.Existing.DiscontinueDate;
          }
        }

        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier && (
            Equal(export.DprProgram.ProgramState, "NA") || Equal
          (export.DprProgram.ProgramState, "CA")) && entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedAf.SystemGeneratedIdentifier;

          if (!Lt(local.AfPaAllArearsPriorTo.Date,
            entities.Existing.EffectiveDate))
          {
            export.DprProgram.ProgramState = "PA";
          }
          else
          {
            export.DprProgram.ProgramState = "TA";
          }
        }

        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier && (
            Equal(export.DprProgram.ProgramState, "NA") || Equal
          (export.DprProgram.ProgramState, "CA")) && (
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAfi.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFci.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedMai.SystemGeneratedIdentifier))
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedMai.SystemGeneratedIdentifier)
          {
            // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
            // the same as the NAI program
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNai.SystemGeneratedIdentifier;
          }
          else
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
          }

          export.DprProgram.ProgramState = "";
        }

        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier && entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedAf.SystemGeneratedIdentifier;

          if (!Lt(local.AfPaAllArearsPriorTo.Date,
            entities.Existing.EffectiveDate))
          {
            export.DprProgram.ProgramState = "PA";
          }
          else
          {
            export.DprProgram.ProgramState = "TA";
          }
        }

        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier && entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFc.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNai.SystemGeneratedIdentifier;
          export.DprProgram.ProgramState = "";
        }

        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier && (
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAfi.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFci.SystemGeneratedIdentifier))
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;

          if (export.KeyOnly.SystemGeneratedIdentifier == local
            .HardcodedNa.SystemGeneratedIdentifier)
          {
            export.DprProgram.ProgramState = "NA";
          }
        }

        // 11/01/08  GVandy  CQ#468  If derivation is currently AF-TA and the 
        // new program is not AF then the derivation should change to NA-CA.
        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "TA") && entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNa.SystemGeneratedIdentifier;
          export.DprProgram.ProgramState = "CA";
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
      // -- Find all person program records which became effective after 10/01/
      // 2009.
      //    We will process these records using the new Distribution 2009 
      // matrix.
      foreach(var item in ReadPersonProgramProgram4())
      {
        if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "PA"))
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  AF-PA  ||  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  AF-PA  |  AF-
          // PA  |  AF-PA  |  AF-PA  |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains AF-PA 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedFc.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "PA"))
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  FC-PA  ||  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  FC-PA  |  FC-
          // PA  |  FC-PA  |  FC-PA  |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains FC-PA 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "NA"))
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  NA-NA  ||  NA-NA  |  NA-NA  |  NA-NA  |  AFI    |  FCI    |  NAI
          // |  NA-NA  |  NA-NA  |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedAfi.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedAfi.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedFci.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedFci.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedNai.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedMai.SystemGeneratedIdentifier)
          {
            // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
            // the same as the NAI program
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNai.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else
          {
            // -- No processing required.  The debt derivation remains NA-NA for
            // all non interstate programs.
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAfi.SystemGeneratedIdentifier)
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  AFI    ||  AFI    |  AFI    |  AFI    |  AFI    |  AFI    |  AFI
          // |  AFI    |  AFI    |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains AFI 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedFci.SystemGeneratedIdentifier)
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  FCI    ||  FCI    |  FCI    |  FCI    |  FCI    |  FCI    |  FCI
          // |  FCI    |  FCI    |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains FCI 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier)
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  NAI    ||  NAI    |  NAI    |  NA-NA  |  AFI    |  FCI    |  NAI
          // |  NAI    |  NAI    |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedNa.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNa.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "NA";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedAfi.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedAfi.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedFci.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedFci.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else
          {
            // -- No processing required.  The debt derivation remains NAI for 
            // all other programs.
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNc.SystemGeneratedIdentifier)
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  NC     ||  NC     |  NC     |  NC     |  NC     |  NC     |  NC
          // |  NC     |  NC     |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains NC 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNf.SystemGeneratedIdentifier)
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  NF     ||  NF     |  NF     |  NF     |  NF     |  NF     |  NF
          // |  NF     |  NF     |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // -- No processing required.  The debt derivation remains NF 
          // regardless of the new program.
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "CA"))
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  NA-CA  ||  NA-CA  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  NAI
          // |  NA-CA  |  NA-CA  |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedAfi.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedAfi.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedFci.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedFci.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedNai.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedMai.SystemGeneratedIdentifier)
          {
            // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
            // the same as the NAI program
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNai.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else
          {
            // -- No processing required.  The debt derivation remains NA-CA for
            // all non interstate programs.
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier && Equal
          (export.DprProgram.ProgramState, "TA"))
        {
          // Arrears
          // were:      To new program:
          //            +---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          //            |  AF     |  FC     |  NA     |  AFI    |  FCI    |  NAI
          // |  NF     |  NC     |
          // +=========++=========+=========+=========+=========+=========+
          // =========+=========+=========+
          // |  AF-TA  ||  **N/A  |  NA-CA  |  NA-CA  |  AFI    |  FCI    |  NAI
          // |  NA-CA  |  NA-CA  |
          // +---------++---------+---------+---------+---------+---------+
          // ---------+---------+---------+
          // ** If the TAF program is active as of 10/1/2009, the AF-TA arrears 
          // will remain AF-TA until
          //    the TAF program closes.  At that point, the arrears become NA-CA
          // and will not change,
          //    even if the TAF program becomes active again.
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            // -- No processing required.  The debt derivation remains AF-TA 
            // unless the TAF program in effect on 10/01/2009 has closed.
            //    We'll check for the closure of that program following this 
            // READ EACH, if the derivation is still AF-TA at that time.
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedAfi.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedAfi.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedFci.SystemGeneratedIdentifier)
          {
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedFci.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier ==
            local.HardcodedNai.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedMai.SystemGeneratedIdentifier)
          {
            // 11/01/08  GVandy  CQ#500 MAI program should result in derivation 
            // the same as the NAI program
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNai.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "";
          }
          else
          {
            // -- The debt derivation changes to NA-CA for all other programs.
            export.KeyOnly.SystemGeneratedIdentifier =
              local.HardcodedNa.SystemGeneratedIdentifier;
            export.DprProgram.ProgramState = "CA";
          }
        }

        // 11/01/08  GVandy  CQ#468  Added the following statements to track the
        // maximum AF discontinue date.
        // Will need this following this READ EACH when determining whether to 
        // change an AF-TA derivation to NA-CA due AF closure.
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          if (Lt(local.MaxAfDiscDt.Date, entities.Existing.DiscontinueDate))
          {
            local.MaxAfDiscDt.Date = entities.Existing.DiscontinueDate;
          }
        }
      }

      // --  If PRWORA derivation is AF-TA check if the TAF program effective on
      // 10/01/2009 has ended.  If it has then the derivation becomes NA-CA.
      if (export.KeyOnly.SystemGeneratedIdentifier == local
        .HardcodedAf.SystemGeneratedIdentifier && Equal
        (export.DprProgram.ProgramState, "TA"))
      {
        // --  Added check for program effective date less than distribution 
        // 2009 start date and discontinue date greater than distribution 2009
        // start date.
        if (ReadPersonProgramProgram2())
        {
          // -- The AF program which was effective on 10/01/2009 is still 
          // effective, so we leave the derivation at AF-TA.
          goto Test2;
        }

        // -- If we get to this point then the AF program which was effective on
        // 10/01/2009 is no longer effective.  Change the derivation from AF-TA
        // to NA-CA.
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNa.SystemGeneratedIdentifier;
        export.DprProgram.ProgramState = "CA";
      }
    }

Test2:

    // *******************************************************************
    // Handle the situation where the program is derived as AF-TA and the
    // AF program has actually ended prior to the date of collection.
    // If this is the case the AF-TA should be NA-CA .
    // (11/01/2008 - Moved this logic from the PRWORA matrix processing
    // section to here because there might have been another AF timeframe
    // starting after 10/01/2009 which means that the derivation should
    // remain AF-TA...)
    // *******************************************************************
    if (export.KeyOnly.SystemGeneratedIdentifier == local
      .HardcodedAf.SystemGeneratedIdentifier && Equal
      (export.DprProgram.ProgramState, "TA"))
    {
      // 11/01/08  GVandy  CQ#468 If derivation is currently AF-TA and the max 
      // AF closure date is less than the date of collection then the
      // derivation should change to NA-CA.
      if (Lt(local.MaxAfDiscDt.Date, import.Collection.Date))
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNa.SystemGeneratedIdentifier;
        export.DprProgram.ProgramState = "CA";
      }
    }

    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // @@@
    // 
    // @@@
    // @@@
    // 
    // @@@
    // @@@   ORIGINAL CODE BELOW IN ITS ENTIRETY  11/01/2008            @@@
    // @@@
    // 
    // @@@
    // @@@
    // 
    // @@@
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
  }

  private void UseFnDetermineProgramState()
  {
    var useImport = new FnDetermineProgramState.Import();
    var useExport = new FnDetermineProgramState.Export();

    useImport.KeyOnly.SystemGeneratedIdentifier =
      export.KeyOnly.SystemGeneratedIdentifier;

    Call(FnDetermineProgramState.Execute, useImport, useExport);

    export.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DateOfEmancipation.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          local.Distribution2009Start.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.Collection.Date.GetValueOrDefault());
        db.SetInt32(
          command, "programId", local.HardcodedAf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedNai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedMai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier9",
          local.HardcodedNf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram4()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.Collection.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate3",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate4",
          local.Distribution2009Start.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedNai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedMai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier9",
          local.HardcodedNf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram5()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.Collection.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate3",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate4",
          local.Distribution2009Start.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedNai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedMai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier9",
          local.HardcodedNf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;

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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CsePerson DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of DateOfEmancipation.
    /// </summary>
    [JsonPropertyName("dateOfEmancipation")]
    public DateWorkArea DateOfEmancipation
    {
      get => dateOfEmancipation ??= new();
      set => dateOfEmancipation = value;
    }

    private DprProgram dprProgram;
    private CsePerson supportedPerson;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private Program keyOnly;
    private DateWorkArea collection;
    private DateWorkArea debtDue;
    private CsePerson delMe;
    private DateWorkArea dateOfEmancipation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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

    private Program keyOnly;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedNa.
    /// </summary>
    [JsonPropertyName("hardcodedNa")]
    public Program HardcodedNa
    {
      get => hardcodedNa ??= new();
      set => hardcodedNa = value;
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
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
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
    /// A value of MaxAfDiscDt.
    /// </summary>
    [JsonPropertyName("maxAfDiscDt")]
    public DateWorkArea MaxAfDiscDt
    {
      get => maxAfDiscDt ??= new();
      set => maxAfDiscDt = value;
    }

    private DateWorkArea distribution2009Start;
    private DateWorkArea nullDate;
    private DateWorkArea afPaAllArearsPriorTo;
    private Program hardcodedAf;
    private Program hardcodedNa;
    private Program hardcodedAfi;
    private Program hardcodedNai;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DateWorkArea maxAfDiscDt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public PersonProgram Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyProgram.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyProgram")]
    public Program ExistingKeyOnlyProgram
    {
      get => existingKeyOnlyProgram ??= new();
      set => existingKeyOnlyProgram = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCsePerson")]
    public CsePerson ExistingKeyOnlyCsePerson
    {
      get => existingKeyOnlyCsePerson ??= new();
      set => existingKeyOnlyCsePerson = value;
    }

    /// <summary>
    /// A value of DeleteMe.
    /// </summary>
    [JsonPropertyName("deleteMe")]
    public Program DeleteMe
    {
      get => deleteMe ??= new();
      set => deleteMe = value;
    }

    private PersonProgram existing;
    private Program existingKeyOnlyProgram;
    private CsePerson existingKeyOnlyCsePerson;
    private Program deleteMe;
  }
#endregion
}
