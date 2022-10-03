// Program: CO_CDNL_LIST_CODE_NAMES, ID: 371823068, model: 746.
// Short name: SWECDNLP
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
/// A program: CO_CDNL_LIST_CODE_NAMES.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step lists occurrences of entity CODE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCdnlListCodeNames: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CDNL_LIST_CODE_NAMES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCdnlListCodeNames(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCdnlListCodeNames.
  /// </summary>
  public CoCdnlListCodeNames(IContext context, Import import, Export export):
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
    // MAINTENANCE LOG
    // AUTHOR     	DATE	        CHG REQ# - DESCRIPTION
    // Govindaraj 	1-18-1995	Initial coding
    // A. Hackler	1/23/96         ADD SECURITY/NEXTRAN
    // R. Marchman	10/14/96        Add data level security
    // A.Kinney	04/28/97	Changed Current_Date
    // *********************************************
    // *********************************************
    // SYSTEM:      KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION: This procedure step lists the code names.
    // PROCESSING:
    // If input "List active values only" = "Y" then only the current values are
    // listed. Otherwise all the values are listed.
    // If a starting code name is specified, then only those with value greater 
    // or equal to the given starting code name are listed.
    // The procedure step tries to read and list a maximum of 100 values at a 
    // time. To list beyond those 100 entries, a new starting value needs to be
    // specified.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // CODE	                 - R - -
    // DATABASE FILES USED:
    // CREATED BY: Govindaraj Kadambi
    // DATE CREATED: 01-18-1995.
    // *********************************************
    local.Current.Date = Now().Date;

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    export.DisplayActiveCasesOnly.Flag = import.DisplayActiveCasesOnly.Flag;
    export.Starting.CodeName = import.Starting.CodeName;

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        if (import.Import1.Item.Detail.Id == 0)
        {
          export.Export1.Next();

          goto Test;
        }

        export.Export1.Update.DetailSelection.SelectChar =
          import.Import1.Item.DetailSelection.SelectChar;
        export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
        export.Export1.Next();
      }
    }

Test:

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "DISPLAY":
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCode())
        {
          export.Export1.Update.Detail.Assign(entities.Existing);
          local.DateWorkArea.Date = entities.Existing.ExpirationDate;
          UseCabSetMaximumDiscontinueDate();
          export.Export1.Update.Detail.ExpirationDate = local.DateWorkArea.Date;
          export.Export1.Next();
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailSelection.SelectChar) && AsChar
            (export.Export1.Item.DetailSelection.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailSelection, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

            return;
          }

          if (AsChar(export.Export1.Item.DetailSelection.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field =
                GetField(export.Export1.Item.DetailSelection, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

              return;
            }

            export.DlgflwSelected.Assign(export.Export1.Item.Detail);
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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

  private IEnumerable<bool> ReadCode()
  {
    return ReadEach("ReadCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Starting.CodeName);
        db.SetString(command, "flag", export.DisplayActiveCasesOnly.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Existing.Id = db.GetInt32(reader, 0);
        entities.Existing.CodeName = db.GetString(reader, 1);
        entities.Existing.EffectiveDate = db.GetDate(reader, 2);
        entities.Existing.ExpirationDate = db.GetDate(reader, 3);
        entities.Existing.DisplayTitle = db.GetString(reader, 4);
        entities.Existing.Populated = true;

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
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common detailSelection;
      private Code detail;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Code Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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

    private Code starting;
    private Common displayActiveCasesOnly;
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
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common detailSelection;
      private Code detail;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public Code DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Code Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Code dlgflwSelected;
    private Code starting;
    private Common displayActiveCasesOnly;
    private Array<ExportGroup> export1;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
    private Common selected;
    private NextTranInfo nextTranInfo;
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
    public Code Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Code existing;
  }
#endregion
}
