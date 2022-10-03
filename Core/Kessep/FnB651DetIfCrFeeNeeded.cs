// Program: FN_B651_DET_IF_CR_FEE_NEEDED, ID: 373525725, model: 746.
// Short name: SWE02603
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_DET_IF_CR_FEE_NEEDED.
/// </summary>
[Serializable]
public partial class FnB651DetIfCrFeeNeeded: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_DET_IF_CR_FEE_NEEDED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651DetIfCrFeeNeeded(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651DetIfCrFeeNeeded.
  /// </summary>
  public FnB651DetIfCrFeeNeeded(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2000-02-14  PR 86861  Fangman  Created for determining if cost recovery 
    // fee is to be taken.
    // 01/05/2001 SWSRKXD PR 99173
    // Cater for Emancipated CHildren case roles.
    // 2001-03-12  PR 115428  Fangman  Take out code for Emancipated CHildren 
    // case roles.
    // ***************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 10/22/2010  Raj S              CQ9426      Modified to add Public 
    // Assistance Code   *
    // *
    // 
    // (PA) CI  (value 8) to the Cost Recovery*
    // *
    // 
    // Fee exempted list for both AR & CH       *
    // *
    // 
    // *
    // ***************************************************************************************
    export.CrFeeNeededInd.Flag = "Y";
    local.EabReportSend.RptDetail = "";

    // Check the active programs for the AR on the collection date.
    foreach(var item in ReadPersonProgramProgram1())
    {
      // We do not take CR Fees for AF, AFI, CC, FS, MA, MAI, MK, MP, MS,SI or 
      // CI.
      // ***************************************************************************************
      // * CQ9426: Add program CI to exempt list
      // 
      // *
      // ***************************************************************************************
      if (entities.Program.SystemGeneratedIdentifier == 2 || entities
        .Program.SystemGeneratedIdentifier == 14 || entities
        .Program.SystemGeneratedIdentifier == 5 || entities
        .Program.SystemGeneratedIdentifier == 4 || entities
        .Program.SystemGeneratedIdentifier == 6 || entities
        .Program.SystemGeneratedIdentifier == 17 || entities
        .Program.SystemGeneratedIdentifier == 11 || entities
        .Program.SystemGeneratedIdentifier == 10 || entities
        .Program.SystemGeneratedIdentifier == 7 || entities
        .Program.SystemGeneratedIdentifier == 9 || entities
        .Program.SystemGeneratedIdentifier == 8)
      {
        export.CrFeeNeededInd.Flag = "N";
        local.EabReportSend.RptDetail = "PA found for AR " + entities
          .ArCsePerson.Number;

        break;
      }
    }

    // Now we are going to look at all AF & MA programs for the AR that were 
    // ever in effect.
    if (IsEmpty(local.EabReportSend.RptDetail))
    {
      foreach(var item in ReadPersonProgramProgram3())
      {
        // We do not take CR Fees if there is an active EM or WT.
        if (!Lt(entities.PersonProgram.MedTypeDiscontinueDate,
          import.Collection.CollectionDt) && (
            Equal(entities.PersonProgram.MedType, "EM") || Equal
          (entities.PersonProgram.MedType, "WT")))
        {
          export.CrFeeNeededInd.Flag = "N";
          local.EabReportSend.RptDetail = "EM or WT found for AR " + entities
            .ArCsePerson.Number;

          goto Test1;
        }
      }
    }

Test1:

    // Next we will run the same check for all children on a case with the AR.
    if (IsEmpty(local.EabReportSend.RptDetail))
    {
      foreach(var item in ReadCaseCaseRoleCsePerson())
      {
        // --------------------------------------------------
        // Determine program as of collection date.
        // --------------------------------------------------
        foreach(var item1 in ReadPersonProgramProgram2())
        {
          // We do not take CR Fees for AF, AFI, CC, FS, MA, MAI, MK, MP, MS,SI 
          // or CI.
          // ***************************************************************************************
          // * CQ9426: Add program CI to exempt list
          // 
          // *
          // ***************************************************************************************
          if (entities.Program.SystemGeneratedIdentifier == 2 || entities
            .Program.SystemGeneratedIdentifier == 14 || entities
            .Program.SystemGeneratedIdentifier == 5 || entities
            .Program.SystemGeneratedIdentifier == 4 || entities
            .Program.SystemGeneratedIdentifier == 6 || entities
            .Program.SystemGeneratedIdentifier == 17 || entities
            .Program.SystemGeneratedIdentifier == 11 || entities
            .Program.SystemGeneratedIdentifier == 10 || entities
            .Program.SystemGeneratedIdentifier == 7 || entities
            .Program.SystemGeneratedIdentifier == 9 || entities
            .Program.SystemGeneratedIdentifier == 8)
          {
            export.CrFeeNeededInd.Flag = "N";
            local.EabReportSend.RptDetail = "PA found for child " + entities
              .ChildCsePerson.Number;

            goto Test2;
          }
        }

        // Now we are going to look at all AF & MA programs for the CHild that 
        // were ever in effect.
        foreach(var item1 in ReadPersonProgramProgram4())
        {
          // We do not take CR Fees if there is an active EM or WT.
          if (!Lt(entities.PersonProgram.MedTypeDiscontinueDate,
            import.Collection.CollectionDt) && (
              Equal(entities.PersonProgram.MedType, "EM") || Equal
            (entities.PersonProgram.MedType, "WT")))
          {
            export.CrFeeNeededInd.Flag = "N";
            local.EabReportSend.RptDetail = "EM or WT found for child " + entities
              .ChildCsePerson.Number;

            goto Test2;
          }
        }
      }
    }

Test2:

    if (AsChar(import.TestDisplay.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";

      if (IsEmpty(local.EabReportSend.RptDetail))
      {
        local.EabReportSend.RptDetail = "No PA found for AR or children " + entities
          .ArCsePerson.Number;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson()
  {
    entities.Case1.Populated = false;
    entities.ChildCaseRole.Populated = false;
    entities.ChildCsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetNullableDate(
          command, "startDate",
          import.Collection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ChildCsePerson.Number = db.GetString(reader, 2);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 3);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ChildCaseRole.Populated = true;
        entities.ChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram1()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetDate(
          command, "effectiveDate",
          import.Collection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 6);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram2()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.Collection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 6);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 6);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram4()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 6);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of TestDisplay.
    /// </summary>
    [JsonPropertyName("testDisplay")]
    public Common TestDisplay
    {
      get => testDisplay ??= new();
      set => testDisplay = value;
    }

    private CsePerson ar;
    private Collection collection;
    private Common testDisplay;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CrFeeNeededInd.
    /// </summary>
    [JsonPropertyName("crFeeNeededInd")]
    public Common CrFeeNeededInd
    {
      get => crFeeNeededInd ??= new();
      set => crFeeNeededInd = value;
    }

    private Common crFeeNeededInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
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
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    private CsePerson arCsePerson;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole arCaseRole;
    private Case1 case1;
    private CaseRole childCaseRole;
    private CsePerson childCsePerson;
  }
#endregion
}
