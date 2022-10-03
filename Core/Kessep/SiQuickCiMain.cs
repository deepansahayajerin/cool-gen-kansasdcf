// Program: SI_QUICK_CI_MAIN, ID: 374541231, model: 746.
// Short name: SWEQKCIP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_CI_MAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class SiQuickCiMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CI_MAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickCiMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickCiMain.
  /// </summary>
  public SiQuickCiMain(IContext context, Import import, Export export):
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
    // This procedure step functions as a non-display server
    // and is the target of a QUICK application COM proxy.
    // Any changes to the import/export views of this procedure
    // step MUST be coordinated, as such changes will impact the
    // calling COM proxy.
    // ------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 01/2010    	T. Pierce	# 211		Initial development
    // ----------------------------------------------------------------------------
    UseSiQuickContactInformation();
  }

  private static void MoveQuickErrorMessages(QuickErrorMessages source,
    QuickErrorMessages target)
  {
    target.ErrorMessage = source.ErrorMessage;
    target.ErrorCode = source.ErrorCode;
  }

  private static void MoveQuickWorkerContactInfo(QuickWorkerContactInfo source,
    QuickWorkerContactInfo target)
  {
    target.ContactTypeCode = source.ContactTypeCode;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StateProvince = source.StateProvince;
    target.Zip = source.Zip;
    target.Zip4 = source.Zip4;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhoneExtension = source.WorkPhoneExtension;
    target.EmailAddress = source.EmailAddress;
    target.Name = source.Name;
    target.MainFaxAreaCode = source.MainFaxAreaCode;
    target.MainFaxPhoneNumber = source.MainFaxPhoneNumber;
  }

  private void UseSiQuickContactInformation()
  {
    var useImport = new SiQuickContactInformation.Import();
    var useExport = new SiQuickContactInformation.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickContactInformation.Execute, useImport, useExport);

    MoveQuickErrorMessages(useExport.QuickErrorMessages,
      export.QuickErrorMessages);
    export.Case1.Number = useExport.Case1.Number;
    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
    MoveQuickWorkerContactInfo(useExport.QuickWorkerContactInfo,
      export.QuickWorkerContactInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
    }

    /// <summary>
    /// A value of QuickWorkerContactInfo.
    /// </summary>
    [JsonPropertyName("quickWorkerContactInfo")]
    public QuickWorkerContactInfo QuickWorkerContactInfo
    {
      get => quickWorkerContactInfo ??= new();
      set => quickWorkerContactInfo = value;
    }

    private Case1 case1;
    private QuickCpHeader quickCpHeader;
    private QuickErrorMessages quickErrorMessages;
    private QuickWorkerContactInfo quickWorkerContactInfo;
  }
#endregion
}
