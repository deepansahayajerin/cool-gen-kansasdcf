// Program: SP_LOSP_LIST_OFFC_FOR_SRV_PRVDR, ID: 371782517, model: 746.
// Short name: SWELOSPP
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
/// A program: SP_LOSP_LIST_OFFC_FOR_SRV_PRVDR.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpLospListOffcForSrvPrvdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LOSP_LIST_OFFC_FOR_SRV_PRVDR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLospListOffcForSrvPrvdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLospListOffcForSrvPrvdr.
  /// </summary>
  public SpLospListOffcForSrvPrvdr(IContext context, Import import,
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
    // *******************************************
    // *   P R O G R A M    M A I N T E N A N C E
    // *
    // **  DATE      *  DEVELOPER   *  DESCRIPTION
    // **  04/25/95    R. Grey         Initial Development
    // **  02/06/96    A. Hackler      retro fits
    // **  04/23/96    S. Konkader     Zdel Select fields, remove from screen, 
    // remove RETURN logic
    // **  01/03/97	R. Marchman	Add new security/next tran.
    // 
    // **  09/18/03    B. Lee          PR187764 - the return list was
    // bypassing security and should not have.
    // *******************************************
    // ** 04/16/2004 H. Mace - PR200803 - Corrected problem of return from 
    // Service List (pf9) not bringing back selection. Changes 'rtlist' to
    // uppercase ('RTLIST') when setting Command value. Also, skipped the logic
    // that was spacing out the Import Select character which was 'nullifying'
    // the subsequent If statement.
    // *******************************************
    // ******************************************
    // * Set initial EXIT STATE.
    // ******************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ***************************************
    // * Move all IMPORTs to EXPORTs.
    // ***************************************
    export.SrvprvdrPrompt.Flag = import.SrvprvdrPrompt.Flag;
    export.Search.Assign(import.Search);
    export.FormatNme.FormattedName = import.FormatNme.FormattedName;
    export.HiddenServiceProvider.Assign(import.HiddenServiceProvider);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Office.Assign(import.Import1.Item.Office);
      export.Export1.Update.OfficeAddress.Assign(
        import.Import1.Item.OfficeAddress);
      export.Export1.Update.OfficeServiceProvider.Assign(
        import.Import1.Item.OfficeServiceProvider);
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }

      ExitState = "ACO_NE0000_INVALID_COMMAND";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // ********************************************************************************************************
    // PR187764 - the return list was bypassing security and should not
    // have.
    // ********************************************************************************************************
    // pr# 200803 - HLM - 4/16/2004 - Changed lower case "rtlist" to upper case
    // "RTLIST"
    // ********************************************************************************************************
    if (Equal(global.Command, "RTLIST"))
    {
      local.Prev.Command = "RTLIST";
      global.Command = "DISPLAY";
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *********************************************
    // ** Validate processing rules.
    // *********************************************
    if (Equal(local.Prev.Command, "RTLIST"))
    {
      if (import.HiddenServiceProvider.SystemGeneratedId > 0)
      {
        export.Search.Assign(import.HiddenServiceProvider);
        export.HiddenServiceProvider.Assign(import.HiddenServiceProvider);

        // **************************************************************************
        // pr# 200803 - HLM - 4/16/2004 - commented out SET prompt to Spaces
        // **************************************************************************
      }

      if (AsChar(export.SrvprvdrPrompt.Flag) == 'S')
      {
        export.SrvprvdrPrompt.Flag = "+";
      }
    }

    // *********************************************
    // * Main CASE OF COMMAND.
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RTLIST":
        break;
      case "LIST":
        export.HiddenServiceProvider.Assign(export.Search);

        switch(AsChar(export.SrvprvdrPrompt.Flag))
        {
          case ' ':
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.SrvprvdrPrompt, "flag");

            field.Error = true;

            break;
        }

        break;
      case "DISPLAY":
        if (export.Search.SystemGeneratedId == 0)
        {
          var field = GetField(export.Search, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          UseSpReadOfficeARole();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Search, "systemGeneratedId");

          field.Error = true;

          return;
        }

        local.FormatNme.FirstName = export.Search.FirstName;
        local.FormatNme.LastName = export.Search.LastName;
        local.FormatNme.MiddleInitial = export.Search.MiddleInitial;
        UseSiFormatCsePersonName();
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExport1(SpReadOfficeARole.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.OfficeServiceProvider.Assign(source.OfficeServiceProvider);
    target.Office.Assign(source.Office);
    target.OfficeAddress.Assign(source.OfficeAddress);
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

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.FormatNme);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.FormatNme.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSpReadOfficeARole()
  {
    var useImport = new SpReadOfficeARole.Import();
    var useExport = new SpReadOfficeARole.Export();

    useImport.Search.SystemGeneratedId = export.Search.SystemGeneratedId;

    Call(SpReadOfficeARole.Execute, useImport, useExport);

    export.Search.SystemGeneratedId = useImport.Search.SystemGeneratedId;
    export.Search.Assign(useExport.Search);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Common Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common zdel;
      private OfficeServiceProvider officeServiceProvider;
      private Office office;
      private OfficeAddress officeAddress;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of SrvprvdrPrompt.
    /// </summary>
    [JsonPropertyName("srvprvdrPrompt")]
    public Common SrvprvdrPrompt
    {
      get => srvprvdrPrompt ??= new();
      set => srvprvdrPrompt = value;
    }

    /// <summary>
    /// A value of FormatNme.
    /// </summary>
    [JsonPropertyName("formatNme")]
    public CsePersonsWorkSet FormatNme
    {
      get => formatNme ??= new();
      set => formatNme = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private ServiceProvider hiddenServiceProvider;
    private Common srvprvdrPrompt;
    private CsePersonsWorkSet formatNme;
    private ServiceProvider search;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private OfficeServiceProvider officeServiceProvider;
      private Office office;
      private OfficeAddress officeAddress;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of SrvprvdrPrompt.
    /// </summary>
    [JsonPropertyName("srvprvdrPrompt")]
    public Common SrvprvdrPrompt
    {
      get => srvprvdrPrompt ??= new();
      set => srvprvdrPrompt = value;
    }

    /// <summary>
    /// A value of FormatNme.
    /// </summary>
    [JsonPropertyName("formatNme")]
    public CsePersonsWorkSet FormatNme
    {
      get => formatNme ??= new();
      set => formatNme = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private ServiceProvider hiddenServiceProvider;
    private Common srvprvdrPrompt;
    private CsePersonsWorkSet formatNme;
    private ServiceProvider search;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of FormatNme.
    /// </summary>
    [JsonPropertyName("formatNme")]
    public CsePersonsWorkSet FormatNme
    {
      get => formatNme ??= new();
      set => formatNme = value;
    }

    private Common prev;
    private Common common;
    private CsePersonsWorkSet formatNme;
  }
#endregion
}
