// Program: SP_PFRH_PROGRAM_IND_HISTORY, ID: 371796916, model: 746.
// Short name: SWEPFRHP
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
/// <para>
/// A program: SP_PFRH_PROGRAM_IND_HISTORY.
/// </para>
/// <para>
/// THIS PROCEDURE IS USED TO DISPLAY THE HISTORY OF THE PROGRAM FEES AND CHILD 
/// SUPPORT RETENTION CODES. IT IS ALSO USED TO ADD UPDATE OR TO DELETE PROGRAMS
/// FEES AND RETENTION CODES WHICH HAVE BEEN SET UP WITH AN EFFECTIVE DATE IN
/// THE FUTURE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpPfrhProgramIndHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PFRH_PROGRAM_IND_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPfrhProgramIndHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPfrhProgramIndHistory.
  /// </summary>
  public SpPfrhProgramIndHistory(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 02/18/96     a. hackler   retro fits
    // *********************************************
    // ** 01/04/97	R. Marchman  Add new security/next tran.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** Housekeeping -  rolling import over to export ****
    export.Hidden.Assign(import.Hidden);
    export.Program.Assign(import.Program);
    export.SearchProgramIndicator.EffectiveDate =
      import.SearchProgramIndicator.EffectiveDate;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ZD_CO0000_CLEAR_SUCCESSFUL_1";

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      local.Current.Date = Now().Date;
      local.High.Date = new DateTime(2099, 12, 31);

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.ProgramIndicator.Assign(
          import.Import1.Item.ProgramIndicator);
        export.Export1.Update.Hidden.Assign(import.Import1.Item.Hidden);
        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        export.Export1.Update.ListCsRetentCd.SelectChar =
          import.Import1.Item.ListCsRetentCd.SelectChar;
        export.Export1.Next();
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ****  END OF HOUSEKEEPING  ****
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (IsEmpty(import.Standard.NextTransaction))
        {
          ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";
        }
        else
        {
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "DISPLAY":
        local.Records.Count = 0;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadProgramIndicator())
        {
          export.Export1.Update.ProgramIndicator.Assign(
            entities.ProgramIndicator);
          export.Export1.Update.Hidden.Assign(entities.ProgramIndicator);
          local.DateWorkArea.Date =
            export.Export1.Item.ProgramIndicator.DiscontinueDate;
          export.Export1.Update.ProgramIndicator.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        break;
    }
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadProgramIndicator()
  {
    return ReadEach("ReadProgramIndicator",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.SearchProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ProgramIndicator.ChildSupportRetentionCode =
          db.GetString(reader, 0);
        entities.ProgramIndicator.IvDFeeIndicator = db.GetString(reader, 1);
        entities.ProgramIndicator.EffectiveDate = db.GetDate(reader, 2);
        entities.ProgramIndicator.DiscontinueDate = db.GetDate(reader, 3);
        entities.ProgramIndicator.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ProgramIndicator.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of ListCsRetentCd.
      /// </summary>
      [JsonPropertyName("listCsRetentCd")]
      public Common ListCsRetentCd
      {
        get => listCsRetentCd ??= new();
        set => listCsRetentCd = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ProgramIndicator Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
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
      /// A value of ProgramIndicator.
      /// </summary>
      [JsonPropertyName("programIndicator")]
      public ProgramIndicator ProgramIndicator
      {
        get => programIndicator ??= new();
        set => programIndicator = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common listCsRetentCd;
      private ProgramIndicator hidden;
      private DateWorkArea dateWorkArea;
      private Common common;
      private ProgramIndicator programIndicator;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of SearchDateWorkArea.
    /// </summary>
    [JsonPropertyName("searchDateWorkArea")]
    public DateWorkArea SearchDateWorkArea
    {
      get => searchDateWorkArea ??= new();
      set => searchDateWorkArea = value;
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
    /// A value of SearchProgramIndicator.
    /// </summary>
    [JsonPropertyName("searchProgramIndicator")]
    public ProgramIndicator SearchProgramIndicator
    {
      get => searchProgramIndicator ??= new();
      set => searchProgramIndicator = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private CodeValue codeValue;
    private Code code;
    private DateWorkArea searchDateWorkArea;
    private Program program;
    private ProgramIndicator searchProgramIndicator;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ProgramIndicator Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of ListCsRetentCd.
      /// </summary>
      [JsonPropertyName("listCsRetentCd")]
      public Common ListCsRetentCd
      {
        get => listCsRetentCd ??= new();
        set => listCsRetentCd = value;
      }

      /// <summary>
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
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
      /// A value of ProgramIndicator.
      /// </summary>
      [JsonPropertyName("programIndicator")]
      public ProgramIndicator ProgramIndicator
      {
        get => programIndicator ??= new();
        set => programIndicator = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ProgramIndicator hidden;
      private Common listCsRetentCd;
      private DateWorkArea dateWorkArea;
      private Common common;
      private ProgramIndicator programIndicator;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of SearchDateWorkArea.
    /// </summary>
    [JsonPropertyName("searchDateWorkArea")]
    public DateWorkArea SearchDateWorkArea
    {
      get => searchDateWorkArea ??= new();
      set => searchDateWorkArea = value;
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
    /// A value of SearchProgramIndicator.
    /// </summary>
    [JsonPropertyName("searchProgramIndicator")]
    public ProgramIndicator SearchProgramIndicator
    {
      get => searchProgramIndicator ??= new();
      set => searchProgramIndicator = value;
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

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public ProgramIndicator HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private CodeValue codeValue;
    private Code code;
    private DateWorkArea searchDateWorkArea;
    private Program program;
    private ProgramIndicator searchProgramIndicator;
    private Array<ExportGroup> export1;
    private ProgramIndicator hiddenSelection;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of High.
    /// </summary>
    [JsonPropertyName("high")]
    public DateWorkArea High
    {
      get => high ??= new();
      set => high = value;
    }

    /// <summary>
    /// A value of ListSelect.
    /// </summary>
    [JsonPropertyName("listSelect")]
    public Common ListSelect
    {
      get => listSelect ??= new();
      set => listSelect = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

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
    /// A value of Records.
    /// </summary>
    [JsonPropertyName("records")]
    public Common Records
    {
      get => records ??= new();
      set => records = value;
    }

    /// <summary>
    /// A value of ProgInd.
    /// </summary>
    [JsonPropertyName("progInd")]
    public ProgramIndicator ProgInd
    {
      get => progInd ??= new();
      set => progInd = value;
    }

    /// <summary>
    /// A value of InitializedProgramIndicator.
    /// </summary>
    [JsonPropertyName("initializedProgramIndicator")]
    public ProgramIndicator InitializedProgramIndicator
    {
      get => initializedProgramIndicator ??= new();
      set => initializedProgramIndicator = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of DiscDateFound.
    /// </summary>
    [JsonPropertyName("discDateFound")]
    public Common DiscDateFound
    {
      get => discDateFound ??= new();
      set => discDateFound = value;
    }

    /// <summary>
    /// A value of EffectDateFound.
    /// </summary>
    [JsonPropertyName("effectDateFound")]
    public Common EffectDateFound
    {
      get => effectDateFound ??= new();
      set => effectDateFound = value;
    }

    /// <summary>
    /// A value of MaxDisDate.
    /// </summary>
    [JsonPropertyName("maxDisDate")]
    public DateWorkArea MaxDisDate
    {
      get => maxDisDate ??= new();
      set => maxDisDate = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    public ProgramIndicator Covered
    {
      get => covered ??= new();
      set => covered = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    private DateWorkArea high;
    private Common listSelect;
    private Common select;
    private DateWorkArea current;
    private Common records;
    private ProgramIndicator progInd;
    private ProgramIndicator initializedProgramIndicator;
    private DateWorkArea initializedDateWorkArea;
    private Common discDateFound;
    private Common effectDateFound;
    private DateWorkArea maxDisDate;
    private Common firstTime;
    private ProgramIndicator covered;
    private Common returnCode;
    private DateWorkArea dateWorkArea;
    private Common common;
    private CodeValue codeValue;
    private Code code;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    private Code code;
    private CodeValue codeValue;
    private Program program;
    private ProgramIndicator programIndicator;
  }
#endregion
}
