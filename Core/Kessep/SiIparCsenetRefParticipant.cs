// Program: SI_IPAR_CSENET_REF_PARTICIPANT, ID: 372514123, model: 746.
// Short name: SWEIPARP
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
/// A program: SI_IPAR_CSENET_REF_PARTICIPANT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIparCsenetRefParticipant: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IPAR_CSENET_REF_PARTICIPANT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIparCsenetRefParticipant(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIparCsenetRefParticipant.
  /// </summary>
  public SiIparCsenetRefParticipant(IContext context, Import import,
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
    // Date	  Developer Name	Description
    // 03-01-95  J.W. Hays - MTW	Initial Development
    // 05-08-95  Ken Evans - MTW	Continue development
    // 02-01-96  J. Howard - SRS	Retrofit
    // 11/05/96  G. Lofton - MTW	Add new security and removed 	old.
    // 03/01/99  P. Sharp  - SRC       Added the new fields to meet version 3.0 
    // compliance.
    //                                 
    // Redesigned screens to accomodate
    // fields.
    // 04/03/00  C. Scroggins - ITS    Added view and view matching for family 
    // violence.
    // ------------------------------------------------------------
    // *********************************************
    // This procedure will display the AR, any
    // additional AP's, and children.  A group view
    // is populated with all of the above to pass
    // to the AP ID procedure for NameList.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.MinDate.Date = new DateTime(1, 1, 1);
    export.Hidden.Assign(import.Hidden);
    export.InterstateCase.Assign(import.InterstateCase);
    export.ArInterstateParticipant.Assign(import.ArInterstateParticipant);
    export.ArWorkArea.Text33 = import.ArWorkArea.Text33;
    export.Regi.Number = import.Regi.Number;
    export.RegiNewCase.Flag = import.RegiNewCase.Flag;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    MoveOffice(import.Office, export.Office);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);

    if (!import.ChildrenAp.IsEmpty)
    {
      export.ChildrenAp.Index = 0;
      export.ChildrenAp.Clear();

      for(import.ChildrenAp.Index = 0; import.ChildrenAp.Index < import
        .ChildrenAp.Count; ++import.ChildrenAp.Index)
      {
        if (export.ChildrenAp.IsFull)
        {
          break;
        }

        export.ChildrenAp.Update.ChildrenApInterstateParticipant.Assign(
          import.ChildrenAp.Item.ChildrenApInterstateParticipant);
        export.ChildrenAp.Update.ChildrenApWorkArea.Text33 =
          import.ChildrenAp.Item.ChildrenApWorkArea.Text33;
        export.ChildrenAp.Next();
      }
    }

    if (!import.List.IsEmpty)
    {
      export.List.Index = -1;

      for(import.List.Index = 0; import.List.Index < import.List.Count; ++
        import.List.Index)
      {
        if (!import.List.CheckSize())
        {
          break;
        }

        ++export.List.Index;
        export.List.CheckSize();

        export.List.Update.G.Assign(import.List.Item.G);
        export.List.Update.GdetailCase.Type1 =
          import.List.Item.GdetailCase.Type1;
        export.List.Update.GdetailFamily.Type1 =
          import.List.Item.GdetailFamily.Type1;
      }

      import.List.CheckIndex();
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

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

    if (!IsEmpty(export.InterstateCase.KsCaseId))
    {
      local.Case1.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
    }

    switch(TrimEnd(global.Command))
    {
      case "IAPC":
        break;
      case "IAPH":
        break;
      case "ISUP":
        break;
      case "IMIS":
        break;
      case "SCNX":
        break;
      default:
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "DISPLAY":
        UseSiBuildCsenetParticipantList();

        if (IsExitState("AR_PARTICIPNT_NF") || IsExitState
          ("CSENET_REFERRAL_CHILDREN_NF"))
        {
          return;
        }

        if (Equal(export.ArInterstateParticipant.Ssn, "000000000"))
        {
          export.ArInterstateParticipant.Ssn = "";
        }

        if (Equal(export.ArInterstateParticipant.WorkAreaCode, "000"))
        {
          export.ArInterstateParticipant.WorkAreaCode = "";
        }

        if (Equal(export.ArInterstateParticipant.WorkPhone, "0000000"))
        {
          export.ArInterstateParticipant.WorkPhone = "";
        }

        if (Equal(export.ArInterstateParticipant.ZipCode5, "00000"))
        {
          export.ArInterstateParticipant.ZipCode5 = "";
        }

        if (Equal(export.ArInterstateParticipant.ZipCode4, "0000"))
        {
          export.ArInterstateParticipant.ZipCode4 = "";
        }

        // *********************************************
        // Populate the participant group with all
        // members found... to be used in namelist and
        // REGI
        // *********************************************
        // *********************************************
        // Move AR to group
        // *********************************************
        export.List.Index = 0;
        export.List.CheckSize();

        export.List.Update.G.FormattedName = export.ArWorkArea.Text33;
        export.List.Update.G.Dob = export.ArInterstateParticipant.DateOfBirth;
        export.List.Update.G.FirstName =
          export.ArInterstateParticipant.NameFirst ?? Spaces(12);
        export.List.Update.G.LastName =
          export.ArInterstateParticipant.NameLast ?? Spaces(17);
        export.List.Update.G.MiddleInitial =
          export.ArInterstateParticipant.NameMiddle ?? Spaces(1);
        export.List.Update.G.Sex = export.ArInterstateParticipant.Sex ?? Spaces
          (1);
        export.List.Update.G.Ssn = export.ArInterstateParticipant.Ssn ?? Spaces
          (9);
        export.List.Update.GdetailCase.Type1 =
          export.ArInterstateParticipant.Relationship ?? Spaces(2);
        export.List.Update.GdetailFamily.Type1 =
          export.ArInterstateParticipant.DependentRelationCp ?? Spaces(2);

        if (Equal(export.ArInterstateParticipant.EmployerVerifiedDate,
          local.MinDate.Date) || Equal
          (export.ArInterstateParticipant.EmployerVerifiedDate,
          local.NullDate.Date))
        {
          export.ArInterstateParticipant.EmployerConfirmedInd = "";
        }
        else
        {
          export.ArInterstateParticipant.EmployerConfirmedInd = "Y";
        }

        // *********************************************
        // Move children to group
        // *********************************************
        for(export.ChildrenAp.Index = 0; export.ChildrenAp.Index < export
          .ChildrenAp.Count; ++export.ChildrenAp.Index)
        {
          ++export.List.Index;
          export.List.CheckSize();

          export.List.Update.G.FormattedName =
            export.ChildrenAp.Item.ChildrenApWorkArea.Text33;
          export.List.Update.G.Dob =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.DateOfBirth;
          export.List.Update.G.FirstName =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.
              NameFirst ?? Spaces(12);
          export.List.Update.G.LastName =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.NameLast ?? Spaces
            (17);
          export.List.Update.G.MiddleInitial =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.
              NameMiddle ?? Spaces(1);
          export.List.Update.G.Sex =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.Sex ?? Spaces
            (1);
          export.List.Update.G.Ssn =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.Ssn ?? Spaces
            (9);

          if (Equal(export.ChildrenAp.Item.ChildrenApInterstateParticipant.Ssn,
            "000000000"))
          {
            export.ChildrenAp.Update.ChildrenApInterstateParticipant.Ssn = "";
          }

          export.List.Update.GdetailCase.Type1 =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.
              Relationship ?? Spaces(2);
          export.List.Update.GdetailFamily.Type1 =
            export.ChildrenAp.Item.ChildrenApInterstateParticipant.
              DependentRelationCp ?? Spaces(2);
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "IAPC":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApCurrentInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
          }
          else
          {
            ExitState = "CSENET_DATA_DOES_NOT_EXIST";
          }
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "IAPH":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApHistoryInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
          }
          else
          {
            ExitState = "CSENET_DATA_DOES_NOT_EXIST";
          }
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "ISUP":
        if (export.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "IMIS":
        if (export.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "SCNX":
        if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_AP_ID";
        }
        else if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApCurrentInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
          }
        }
        else if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
  }

  private static void MoveListToChildrenAp(SiBuildCsenetParticipantList.Export.
    ListGroup source, Export.ChildrenApGroup target)
  {
    target.ChildrenApWorkArea.Text33 = source.Gnames.Text33;
    target.ChildrenApInterstateParticipant.Assign(source.G);
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

    useImport.Case1.Number = local.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiBuildCsenetParticipantList()
  {
    var useImport = new SiBuildCsenetParticipantList.Import();
    var useExport = new SiBuildCsenetParticipantList.Export();

    MoveInterstateCase2(import.InterstateCase, useImport.InterstateCase);

    Call(SiBuildCsenetParticipantList.Execute, useImport, useExport);

    export.ArWorkArea.Text33 = useExport.ArName.Text33;
    export.ArInterstateParticipant.Assign(useExport.Ar);
    useExport.List.CopyTo(export.ChildrenAp, MoveListToChildrenAp);
  }

  private void UseSiCheckApCurrHist()
  {
    var useImport = new SiCheckApCurrHist.Import();
    var useExport = new SiCheckApCurrHist.Export();

    MoveInterstateCase1(import.InterstateCase, useImport.InterstateCase);

    Call(SiCheckApCurrHist.Execute, useImport, useExport);

    local.ApCurrentInd.Flag = useExport.ApCurrentInd.Flag;
    local.ApHistoryInd.Flag = useExport.ApHistoryInd.Flag;
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
    /// <summary>A ChildrenApGroup group.</summary>
    [Serializable]
    public class ChildrenApGroup
    {
      /// <summary>
      /// A value of ChildrenApWorkArea.
      /// </summary>
      [JsonPropertyName("childrenApWorkArea")]
      public WorkArea ChildrenApWorkArea
      {
        get => childrenApWorkArea ??= new();
        set => childrenApWorkArea = value;
      }

      /// <summary>
      /// A value of ChildrenApInterstateParticipant.
      /// </summary>
      [JsonPropertyName("childrenApInterstateParticipant")]
      public InterstateParticipant ChildrenApInterstateParticipant
      {
        get => childrenApInterstateParticipant ??= new();
        set => childrenApInterstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea childrenApWorkArea;
      private InterstateParticipant childrenApInterstateParticipant;
    }

    /// <summary>A ZdelImportGroupChildrenApGroup group.</summary>
    [Serializable]
    public class ZdelImportGroupChildrenApGroup
    {
      /// <summary>
      /// A value of ZdelImportGChAp.
      /// </summary>
      [JsonPropertyName("zdelImportGChAp")]
      public WorkArea ZdelImportGChAp
      {
        get => zdelImportGChAp ??= new();
        set => zdelImportGChAp = value;
      }

      /// <summary>
      /// A value of ZdelImportGChildrenAp.
      /// </summary>
      [JsonPropertyName("zdelImportGChildrenAp")]
      public InterstateParticipant ZdelImportGChildrenAp
      {
        get => zdelImportGChildrenAp ??= new();
        set => zdelImportGChildrenAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private WorkArea zdelImportGChAp;
      private InterstateParticipant zdelImportGChildrenAp;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GdetailCase.
      /// </summary>
      [JsonPropertyName("gdetailCase")]
      public CaseRole GdetailCase
      {
        get => gdetailCase ??= new();
        set => gdetailCase = value;
      }

      /// <summary>
      /// A value of GdetailFamily.
      /// </summary>
      [JsonPropertyName("gdetailFamily")]
      public CaseRole GdetailFamily
      {
        get => gdetailFamily ??= new();
        set => gdetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet g;
      private CaseRole gdetailCase;
      private CaseRole gdetailFamily;
    }

    /// <summary>
    /// Gets a value of ChildrenAp.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenApGroup> ChildrenAp => childrenAp ??= new(
      ChildrenApGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildrenAp for json serialization.
    /// </summary>
    [JsonPropertyName("childrenAp")]
    [Computed]
    public IList<ChildrenApGroup> ChildrenAp_Json
    {
      get => childrenAp;
      set => ChildrenAp.Assign(value);
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of ArWorkArea.
    /// </summary>
    [JsonPropertyName("arWorkArea")]
    public WorkArea ArWorkArea
    {
      get => arWorkArea ??= new();
      set => arWorkArea = value;
    }

    /// <summary>
    /// A value of ArInterstateParticipant.
    /// </summary>
    [JsonPropertyName("arInterstateParticipant")]
    public InterstateParticipant ArInterstateParticipant
    {
      get => arInterstateParticipant ??= new();
      set => arInterstateParticipant = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of ZdelImportGroupChildrenAp.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelImportGroupChildrenApGroup> ZdelImportGroupChildrenAp =>
      zdelImportGroupChildrenAp ??= new(ZdelImportGroupChildrenApGroup.Capacity,
      0);

    /// <summary>
    /// Gets a value of ZdelImportGroupChildrenAp for json serialization.
    /// </summary>
    [JsonPropertyName("zdelImportGroupChildrenAp")]
    [Computed]
    public IList<ZdelImportGroupChildrenApGroup> ZdelImportGroupChildrenAp_Json
    {
      get => zdelImportGroupChildrenAp;
      set => ZdelImportGroupChildrenAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private Array<ChildrenApGroup> childrenAp;
    private Case1 regi;
    private Common regiNewCase;
    private WorkArea arWorkArea;
    private InterstateParticipant arInterstateParticipant;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<ZdelImportGroupChildrenApGroup> zdelImportGroupChildrenAp;
    private Array<ListGroup> list;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ChildrenApGroup group.</summary>
    [Serializable]
    public class ChildrenApGroup
    {
      /// <summary>
      /// A value of ChildrenApWorkArea.
      /// </summary>
      [JsonPropertyName("childrenApWorkArea")]
      public WorkArea ChildrenApWorkArea
      {
        get => childrenApWorkArea ??= new();
        set => childrenApWorkArea = value;
      }

      /// <summary>
      /// A value of ChildrenApInterstateParticipant.
      /// </summary>
      [JsonPropertyName("childrenApInterstateParticipant")]
      public InterstateParticipant ChildrenApInterstateParticipant
      {
        get => childrenApInterstateParticipant ??= new();
        set => childrenApInterstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea childrenApWorkArea;
      private InterstateParticipant childrenApInterstateParticipant;
    }

    /// <summary>A ZdelExportGroupChildrenApGroup group.</summary>
    [Serializable]
    public class ZdelExportGroupChildrenApGroup
    {
      /// <summary>
      /// A value of ZdelExportGChAp.
      /// </summary>
      [JsonPropertyName("zdelExportGChAp")]
      public WorkArea ZdelExportGChAp
      {
        get => zdelExportGChAp ??= new();
        set => zdelExportGChAp = value;
      }

      /// <summary>
      /// A value of ZdelExportGChildrenAp.
      /// </summary>
      [JsonPropertyName("zdelExportGChildrenAp")]
      public InterstateParticipant ZdelExportGChildrenAp
      {
        get => zdelExportGChildrenAp ??= new();
        set => zdelExportGChildrenAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private WorkArea zdelExportGChAp;
      private InterstateParticipant zdelExportGChildrenAp;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GdetailCase.
      /// </summary>
      [JsonPropertyName("gdetailCase")]
      public CaseRole GdetailCase
      {
        get => gdetailCase ??= new();
        set => gdetailCase = value;
      }

      /// <summary>
      /// A value of GdetailFamily.
      /// </summary>
      [JsonPropertyName("gdetailFamily")]
      public CaseRole GdetailFamily
      {
        get => gdetailFamily ??= new();
        set => gdetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet g;
      private CaseRole gdetailCase;
      private CaseRole gdetailFamily;
    }

    /// <summary>
    /// Gets a value of ChildrenAp.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenApGroup> ChildrenAp => childrenAp ??= new(
      ChildrenApGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildrenAp for json serialization.
    /// </summary>
    [JsonPropertyName("childrenAp")]
    [Computed]
    public IList<ChildrenApGroup> ChildrenAp_Json
    {
      get => childrenAp;
      set => ChildrenAp.Assign(value);
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of ArWorkArea.
    /// </summary>
    [JsonPropertyName("arWorkArea")]
    public WorkArea ArWorkArea
    {
      get => arWorkArea ??= new();
      set => arWorkArea = value;
    }

    /// <summary>
    /// A value of ArInterstateParticipant.
    /// </summary>
    [JsonPropertyName("arInterstateParticipant")]
    public InterstateParticipant ArInterstateParticipant
    {
      get => arInterstateParticipant ??= new();
      set => arInterstateParticipant = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of ZdelExportGroupChildrenAp.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelExportGroupChildrenApGroup> ZdelExportGroupChildrenAp =>
      zdelExportGroupChildrenAp ??= new(ZdelExportGroupChildrenApGroup.Capacity,
      0);

    /// <summary>
    /// Gets a value of ZdelExportGroupChildrenAp for json serialization.
    /// </summary>
    [JsonPropertyName("zdelExportGroupChildrenAp")]
    [Computed]
    public IList<ZdelExportGroupChildrenApGroup> ZdelExportGroupChildrenAp_Json
    {
      get => zdelExportGroupChildrenAp;
      set => ZdelExportGroupChildrenAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private Array<ChildrenApGroup> childrenAp;
    private Case1 regi;
    private Common regiNewCase;
    private WorkArea arWorkArea;
    private InterstateParticipant arInterstateParticipant;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<ZdelExportGroupChildrenApGroup> zdelExportGroupChildrenAp;
    private Array<ListGroup> list;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of MinDate.
    /// </summary>
    [JsonPropertyName("minDate")]
    public DateWorkArea MinDate
    {
      get => minDate ??= new();
      set => minDate = value;
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
    /// A value of GroupExport.
    /// </summary>
    [JsonPropertyName("groupExport")]
    public Common GroupExport
    {
      get => groupExport ??= new();
      set => groupExport = value;
    }

    /// <summary>
    /// A value of ApCurrentInd.
    /// </summary>
    [JsonPropertyName("apCurrentInd")]
    public Common ApCurrentInd
    {
      get => apCurrentInd ??= new();
      set => apCurrentInd = value;
    }

    /// <summary>
    /// A value of ApHistoryInd.
    /// </summary>
    [JsonPropertyName("apHistoryInd")]
    public Common ApHistoryInd
    {
      get => apHistoryInd ??= new();
      set => apHistoryInd = value;
    }

    private DateWorkArea nullDate;
    private DateWorkArea minDate;
    private Case1 case1;
    private Common groupExport;
    private Common apCurrentInd;
    private Common apHistoryInd;
  }
#endregion
}
