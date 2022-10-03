// Program: FN_AB_OBLIGOR_LIST_FOR_CT_ORDER, ID: 371770027, model: 746.
// Short name: SWE02142
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_AB_OBLIGOR_LIST_FOR_CT_ORDER.
/// </para>
/// <para>
/// This AB lists all the Obligors for a given Court Order #
/// </para>
/// </summary>
[Serializable]
public partial class FnAbObligorListForCtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_OBLIGOR_LIST_FOR_CT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbObligorListForCtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbObligorListForCtOrder.
  /// </summary>
  public FnAbObligorListForCtOrder(IContext context, Import import,
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
    // --------------------------------------------------
    // This AB gives the list of obligor information for a given court order #
    // --------------------------------------------------
    export.WorkNoOfObligors.Count = 0;

    export.ObligorList.Index = 0;
    export.ObligorList.Clear();

    foreach(var item in ReadCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++export.WorkNoOfObligors.Count;
        export.ObligorList.Update.GrpsWork.Assign(local.CsePersonsWorkSet);
        export.ObligorList.Update.Grps.Assign(local.Work);
      }
      else
      {
        export.ObligorList.Next();

        return;
      }

      export.ObligorList.Next();
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.Work);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        if (export.ObligorList.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ObligorListGroup group.</summary>
    [Serializable]
    public class ObligorListGroup
    {
      /// <summary>
      /// A value of Grps.
      /// </summary>
      [JsonPropertyName("grps")]
      public CsePerson Grps
      {
        get => grps ??= new();
        set => grps = value;
      }

      /// <summary>
      /// A value of GrpsWork.
      /// </summary>
      [JsonPropertyName("grpsWork")]
      public CsePersonsWorkSet GrpsWork
      {
        get => grpsWork ??= new();
        set => grpsWork = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePerson grps;
      private CsePersonsWorkSet grpsWork;
    }

    /// <summary>
    /// A value of WorkNoOfObligors.
    /// </summary>
    [JsonPropertyName("workNoOfObligors")]
    public Common WorkNoOfObligors
    {
      get => workNoOfObligors ??= new();
      set => workNoOfObligors = value;
    }

    /// <summary>
    /// Gets a value of ObligorList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorListGroup> ObligorList => obligorList ??= new(
      ObligorListGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligorList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorList")]
    [Computed]
    public IList<ObligorListGroup> ObligorList_Json
    {
      get => obligorList;
      set => ObligorList.Assign(value);
    }

    private Common workNoOfObligors;
    private Array<ObligorListGroup> obligorList;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePerson Work
    {
      get => work ??= new();
      set => work = value;
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

    private CsePerson work;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
