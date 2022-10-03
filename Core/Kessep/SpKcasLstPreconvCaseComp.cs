// Program: SP_KCAS_LST_PRECONV_CASE_COMP, ID: 371751371, model: 746.
// Short name: SWEKCASP
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
/// A program: SP_KCAS_LST_PRECONV_CASE_COMP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpKcasLstPreconvCaseComp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_KCAS_LST_PRECONV_CASE_COMP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpKcasLstPreconvCaseComp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpKcasLstPreconvCaseComp.
  /// </summary>
  public SpKcasLstPreconvCaseComp(IContext context, Import import, Export export)
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
    // _____________________________________________
    //      M A I N T E N A N C E   L O G
    // Date      Developer      Request#        Description
    // 12/30/96  S. Newman-SRS                  Initial Code
    // 01/06/97  R. Marchman		         Add new security/next tran.
    // 09/22/98  SWSRKEH		         Phase 2 changes
    // _____________________________________________
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Maximum.Date = new DateTime(2099, 12, 31);

    // ***Move all IMPORTS to EXPORTS.***
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Adabas.FormattedName = import.Adabas.FormattedName;
    MovePreconvCaseHist1(import.SearchPreconvCaseHist,
      export.SearchPreconvCaseHist);
    export.SearchCsePersonsWorkSet.Number =
      import.SearchCsePersonsWorkSet.Number;
    export.Hidden.Assign(import.Hidden);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.CsePersonsWorkSet.FormattedName =
        import.Import1.Item.CsePersonsWorkSet.FormattedName;

      if (!IsEmpty(import.Import1.Item.Common.SelectChar) && (
        Equal(global.Command, "RETNAME") || Equal(global.Command, "RETNARR")))
      {
        export.Export1.Update.Common.SelectChar = "";
      }
      else
      {
        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
      }

      MovePreconvCaseHist2(import.Import1.Item.PreconvCaseHist,
        export.Export1.Update.PreconvCaseHist);
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    switch(TrimEnd(global.Command))
    {
      case "XXNEXTXX":
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        UseScCabNextTranGet();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "SP0000_PERSON_OR_CASE_NBR_REQD";

        return;
      case "XXFMMENU":
        ExitState = "SP0000_PERSON_OR_CASE_NBR_REQD";

        return;
      case "RETNAME":
        if (!IsEmpty(import.Selected.Number))
        {
          export.SearchCsePersonsWorkSet.Number = import.Selected.Number;
          export.SearchPreconvCaseHist.KaecsesCaseNumber = "";
        }

        global.Command = "DISPLAY";

        break;
      case "RETNARR":
        return;
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          UseScCabNextTranPut();

          return;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        return;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        break;
    }

    if (!Equal(global.Command, "NARR") && !Equal(global.Command, "DISPLAY") && !
      Equal(global.Command, "NAME"))
    {
      ExitState = "ACO_NE0000_INVALID_PF_KEY";

      return;
    }

    // **** the person field or the case number field cannot be spaces and both 
    // cannot be populated ****
    if (!Equal(global.Command, "NAME"))
    {
      if (IsEmpty(export.SearchCsePersonsWorkSet.Number) && IsEmpty
        (export.SearchPreconvCaseHist.KaecsesCaseNumber))
      {
        ExitState = "MANDITORY_FIELDS_NOT_ENTERED";

        var field1 =
          GetField(export.SearchPreconvCaseHist, "kaecsesCaseNumber");

        field1.Error = true;

        var field2 = GetField(export.SearchCsePersonsWorkSet, "number");

        field2.Error = true;

        return;
      }

      if (!IsEmpty(export.SearchCsePersonsWorkSet.Number) && !
        IsEmpty(export.SearchPreconvCaseHist.KaecsesCaseNumber))
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        var field1 =
          GetField(export.SearchPreconvCaseHist, "kaecsesCaseNumber");

        field1.Error = true;

        var field2 = GetField(export.SearchCsePersonsWorkSet, "number");

        field2.Error = true;

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "NARR":
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Common.Count;

              if (!IsEmpty(export.Export1.Item.PreconvCaseHist.KaecsesCaseNumber))
                
              {
              }
              else
              {
                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "REQUEST_CANNOT_BE_EXECUTED";
              }

              break;
            default:
              ++local.Common.Count;

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ***Check to see if a selection has been made.***
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
              {
                export.Export1.Update.Common.SelectChar = "";
                export.FlowPreconvCaseHist.KaecsesCaseNumber =
                  export.Export1.Item.PreconvCaseHist.KaecsesCaseNumber;
                export.FlowCsePersonsWorkSet.Number =
                  export.Export1.Item.PreconvCaseHist.CsePersonNumber;
                ExitState = "ECO_LNK_TO_NARR";

                return;
              }
            }

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
              {
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        break;
      case "DISPLAY":
        if (!IsEmpty(export.SearchCsePersonsWorkSet.Number))
        {
          UseCabZeroFillNumber();

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadPreconvCaseHist1())
          {
            export.Export1.Update.PreconvCaseHist.Assign(
              entities.PreconvCaseHist);
            local.CsePersonsWorkSet.Number =
              export.SearchCsePersonsWorkSet.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_ADABAS_PERSON_NF_113";
            }

            export.Export1.Update.CsePersonsWorkSet.FormattedName =
              local.CsePersonsWorkSet.FormattedName;

            if (Equal(export.Export1.Item.PreconvCaseHist.KaecsesEndDate,
              local.Maximum.Date))
            {
              export.Export1.Update.PreconvCaseHist.KaecsesEndDate =
                local.ZeroValue.Date;
            }

            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          local.WorkArea.Text12 =
            export.SearchPreconvCaseHist.KaecsesCaseNumber;
          UseCabTestForNumericText();

          if (AsChar(local.Common.Flag) == 'Y')
          {
            export.SearchPreconvCaseHist.KaecsesCaseNumber =
              NumberToString(StringToNumber(
                export.SearchPreconvCaseHist.KaecsesCaseNumber), 12);
          }
          else
          {
            ExitState = "CASE_NUMBER_NOT_NUMERIC";

            return;
          }

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadPreconvCaseHist2())
          {
            export.Export1.Update.PreconvCaseHist.Assign(
              entities.PreconvCaseHist);
            local.CsePersonsWorkSet.Number =
              export.Export1.Item.PreconvCaseHist.CsePersonNumber;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_ADABAS_PERSON_NF_113";
            }

            export.Export1.Update.CsePersonsWorkSet.FormattedName =
              local.CsePersonsWorkSet.FormattedName;

            if (Equal(export.Export1.Item.PreconvCaseHist.KaecsesEndDate,
              local.Maximum.Date))
            {
              export.Export1.Update.PreconvCaseHist.KaecsesEndDate =
                local.ZeroValue.Date;
            }

            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MovePreconvCaseHist1(PreconvCaseHist source,
    PreconvCaseHist target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.KaecsesCaseNumber = source.KaecsesCaseNumber;
  }

  private static void MovePreconvCaseHist2(PreconvCaseHist source,
    PreconvCaseHist target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.KaecsesCaseNumber = source.KaecsesCaseNumber;
    target.KaecsesRelationship = source.KaecsesRelationship;
    target.KaecsesStartDate = source.KaecsesStartDate;
    target.KaecsesEndDate = source.KaecsesEndDate;
  }

  private void UseCabTestForNumericText()
  {
    var useImport = new CabTestForNumericText.Import();
    var useExport = new CabTestForNumericText.Export();

    useImport.WorkArea.Text12 = local.WorkArea.Text12;

    Call(CabTestForNumericText.Execute, useImport, useExport);

    local.Common.Flag = useExport.NumericText.Flag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.SearchCsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadPreconvCaseHist1()
  {
    return ReadEach("ReadPreconvCaseHist1",
      (db, command) =>
      {
        db.SetString(
          command, "csePersonNumber", export.SearchCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PreconvCaseHist.CsePersonNumber = db.GetString(reader, 0);
        entities.PreconvCaseHist.KaecsesCaseNumber = db.GetString(reader, 1);
        entities.PreconvCaseHist.KaecsesRelationship = db.GetString(reader, 2);
        entities.PreconvCaseHist.KaecsesStartDate = db.GetDate(reader, 3);
        entities.PreconvCaseHist.KaecsesEndDate = db.GetDate(reader, 4);
        entities.PreconvCaseHist.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PreconvCaseHist.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPreconvCaseHist2()
  {
    return ReadEach("ReadPreconvCaseHist2",
      (db, command) =>
      {
        db.SetString(
          command, "kaecsesCaseNum",
          export.SearchPreconvCaseHist.KaecsesCaseNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PreconvCaseHist.CsePersonNumber = db.GetString(reader, 0);
        entities.PreconvCaseHist.KaecsesCaseNumber = db.GetString(reader, 1);
        entities.PreconvCaseHist.KaecsesRelationship = db.GetString(reader, 2);
        entities.PreconvCaseHist.KaecsesStartDate = db.GetDate(reader, 3);
        entities.PreconvCaseHist.KaecsesEndDate = db.GetDate(reader, 4);
        entities.PreconvCaseHist.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PreconvCaseHist.Populated = true;

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of PreconvCaseHist.
      /// </summary>
      [JsonPropertyName("preconvCaseHist")]
      public PreconvCaseHist PreconvCaseHist
      {
        get => preconvCaseHist ??= new();
        set => preconvCaseHist = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
      private PreconvCaseHist preconvCaseHist;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
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

    /// <summary>
    /// A value of Adabas.
    /// </summary>
    [JsonPropertyName("adabas")]
    public CsePersonsWorkSet Adabas
    {
      get => adabas ??= new();
      set => adabas = value;
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
    /// A value of SearchPreconvCaseHist.
    /// </summary>
    [JsonPropertyName("searchPreconvCaseHist")]
    public PreconvCaseHist SearchPreconvCaseHist
    {
      get => searchPreconvCaseHist ??= new();
      set => searchPreconvCaseHist = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private Standard standard;
    private CsePersonsWorkSet adabas;
    private Array<ImportGroup> import1;
    private PreconvCaseHist searchPreconvCaseHist;
    private NextTranInfo hidden;
    private CsePersonsWorkSet selected;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of PreconvCaseHist.
      /// </summary>
      [JsonPropertyName("preconvCaseHist")]
      public PreconvCaseHist PreconvCaseHist
      {
        get => preconvCaseHist ??= new();
        set => preconvCaseHist = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet csePersonsWorkSet;
      private Common common;
      private PreconvCaseHist preconvCaseHist;
    }

    /// <summary>
    /// A value of FlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("flowCsePersonsWorkSet")]
    public CsePersonsWorkSet FlowCsePersonsWorkSet
    {
      get => flowCsePersonsWorkSet ??= new();
      set => flowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
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

    /// <summary>
    /// A value of Adabas.
    /// </summary>
    [JsonPropertyName("adabas")]
    public CsePersonsWorkSet Adabas
    {
      get => adabas ??= new();
      set => adabas = value;
    }

    /// <summary>
    /// A value of FlowPreconvCaseHist.
    /// </summary>
    [JsonPropertyName("flowPreconvCaseHist")]
    public PreconvCaseHist FlowPreconvCaseHist
    {
      get => flowPreconvCaseHist ??= new();
      set => flowPreconvCaseHist = value;
    }

    /// <summary>
    /// A value of SearchPreconvCaseHist.
    /// </summary>
    [JsonPropertyName("searchPreconvCaseHist")]
    public PreconvCaseHist SearchPreconvCaseHist
    {
      get => searchPreconvCaseHist ??= new();
      set => searchPreconvCaseHist = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private CsePersonsWorkSet flowCsePersonsWorkSet;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private Standard standard;
    private CsePersonsWorkSet adabas;
    private PreconvCaseHist flowPreconvCaseHist;
    private PreconvCaseHist searchPreconvCaseHist;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of ZeroValue.
    /// </summary>
    [JsonPropertyName("zeroValue")]
    public DateWorkArea ZeroValue
    {
      get => zeroValue ??= new();
      set => zeroValue = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea maximum;
    private DateWorkArea zeroValue;
    private Common common;
    private WorkArea workArea;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PreconvCaseHist.
    /// </summary>
    [JsonPropertyName("preconvCaseHist")]
    public PreconvCaseHist PreconvCaseHist
    {
      get => preconvCaseHist ??= new();
      set => preconvCaseHist = value;
    }

    private PreconvCaseHist preconvCaseHist;
  }
#endregion
}
