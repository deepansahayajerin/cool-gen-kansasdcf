// Program: SI_ISUP_CSENET_REF_SUPPORT_ORDER, ID: 372518743, model: 746.
// Short name: SWEISUPP
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
/// A program: SI_ISUP_CSENET_REF_SUPPORT_ORDER.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIsupCsenetRefSupportOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ISUP_CSENET_REF_SUPPORT_ORDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIsupCsenetRefSupportOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIsupCsenetRefSupportOrder.
  /// </summary>
  public SiIsupCsenetRefSupportOrder(IContext context, Import import,
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
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03-01-95  J.W. Hays		Initial Development
    // 05-08-95  Ken Evans		Continue development
    // 01-31-96  Randy Haas		Retro Fit Security
    // 11/05/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // ------------------------------------------------------------
    // *********************************************
    // * The following PRAD displays data found in *
    // * CSENet Referral Support Orders. Actually, *
    // * up to nine Support Orders may be attached *
    // * to every CSENet Referral.  This PRAD      *
    // * displays them all, one at a time.         *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.InterstateCase.Assign(import.InterstateCase);
    export.InterstateSupportOrder.Assign(import.InterstateSupportOrder);
    export.Minus.SelectChar = import.Minus.SelectChar;
    export.Plus.SelectChar = import.Plus.SelectChar;
    export.SubscrSave.Count = import.SubscrSave.Count;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveOffice(import.Office, export.Office);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = -1;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (!import.Import1.CheckSize())
        {
          break;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.G.Assign(import.Import1.Item.G);
      }

      import.Import1.CheckIndex();
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "SCNX"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        UseSiReadCsenetSupportOrders();

        if (export.Export1.IsEmpty)
        {
          ExitState = "REFERRAL_SUPPORT_ORDER_NF";

          return;
        }

        if (export.Export1.Count > 1)
        {
          export.Plus.SelectChar = "+";
        }

        export.Minus.SelectChar = "";

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        export.SubscrSave.Count = 1;
        export.InterstateSupportOrder.Assign(export.Export1.Item.G);

        break;
      case "SCNX":
        if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        if (import.InterstateCase.TransSerialNumber <= 0)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        if (import.SubscrSave.Count > 1)
        {
          export.Export1.Index = import.SubscrSave.Count - 2;
          export.Export1.CheckSize();

          export.SubscrSave.Count = export.Export1.Index + 1;
          export.InterstateSupportOrder.Assign(export.Export1.Item.G);

          if (export.Export1.Index >= 1)
          {
            export.Minus.SelectChar = "-";
          }
          else
          {
            export.Minus.SelectChar = "";
          }

          if (export.Export1.Index + 1 < export.Export1.Count)
          {
            export.Plus.SelectChar = "+";
          }
          else
          {
            export.Plus.SelectChar = "";
          }
        }
        else
        {
          export.Export1.Index = import.SubscrSave.Count - 1;
          export.Export1.CheckSize();

          export.InterstateSupportOrder.Assign(export.Export1.Item.G);
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        break;
      case "NEXT":
        if (import.InterstateCase.TransSerialNumber <= 0)
        {
          ExitState = "ZD_ACO_NE0000_INVALID_FORWARD_3";

          return;
        }

        if (import.SubscrSave.Count < export.Export1.Count)
        {
          export.Export1.Index = export.SubscrSave.Count;
          export.Export1.CheckSize();

          export.SubscrSave.Count = export.Export1.Index + 1;
          export.InterstateSupportOrder.Assign(export.Export1.Item.G);

          if (export.Export1.Index + 1 < export.Export1.Count)
          {
            export.Plus.SelectChar = "+";
          }
          else
          {
            export.Plus.SelectChar = "";
          }

          export.Minus.SelectChar = "-";
        }
        else
        {
          // 2-13-2002 PR138711 added the if statement around the move statement
          // below to eliminate abends when the export group view subscript is
          // 0. Lbachura
          export.Export1.Index = import.SubscrSave.Count - 1;
          export.Export1.CheckSize();

          if (export.SubscrSave.Count != 0)
          {
            export.InterstateSupportOrder.Assign(export.Export1.Item.G);
          }

          ExitState = "NO_MORE_ITEMS_TO_SCROLL";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExport1(SiReadCsenetSupportOrders.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsenetSupportOrders()
  {
    var useImport = new SiReadCsenetSupportOrders.Import();
    var useExport = new SiReadCsenetSupportOrders.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetSupportOrders.Execute, useImport, useExport);

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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Common Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Common Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of SubscrSave.
    /// </summary>
    [JsonPropertyName("subscrSave")]
    public Common SubscrSave
    {
      get => subscrSave ??= new();
      set => subscrSave = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private Common minus;
    private Common plus;
    private Common subscrSave;
    private InterstateCase interstateCase;
    private InterstateSupportOrder interstateSupportOrder;
    private Standard standard;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Common Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Common Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of SubscrSave.
    /// </summary>
    [JsonPropertyName("subscrSave")]
    public Common SubscrSave
    {
      get => subscrSave ??= new();
      set => subscrSave = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private Common minus;
    private Common plus;
    private Common subscrSave;
    private InterstateCase interstateCase;
    private InterstateSupportOrder interstateSupportOrder;
    private Standard standard;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Common Max
    {
      get => max ??= new();
      set => max = value;
    }

    private Common max;
  }
#endregion
}
