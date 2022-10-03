// Program: SC_NEXT_STORE_EXTRACT_OSP, ID: 371930286, model: 746.
// Short name: SWE02038
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SC_NEXT_STORE_EXTRACT_OSP.
/// </para>
/// <para>
/// This cab, built for use in NEXT TRAN logic will store or extract OSP 
/// information.
/// See documentation in CAB for more details.
/// </para>
/// </summary>
[Serializable]
public partial class ScNextStoreExtractOsp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_NEXT_STORE_EXTRACT_OSP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScNextStoreExtractOsp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScNextStoreExtractOsp.
  /// </summary>
  public ScNextStoreExtractOsp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ©©©©©©©©©©
    // ·
    // This cab was built to be used in NEXT TRAN situations where it was 
    // necessary to pass OSP information.
    // Functions available:
    // 1. Convert OSP information into a NEXT TRAN entity text string.
    // 2. Convert NEXT TRAN entity text string to OSP entities.
    // Calling requirements:
    // 1. Import: entity Office Service Provider
    // 		Role Code
    // 		Effective Date
    // 	   entity Service Provider
    // 		System Generated Id
    // 	   entity Office
    // 		System Generated Id
    //     Export: entity NEXT TRAN INFO
    // 		Misc Text 1
    // 2.  The reverse of 1 above.
    // Execution: The function performed by this cab is determined by what 
    // IMPORTs are populated.
    // Order of concatentation is:
    // 			OSP Effective Date  (1 - 7) in Julian
    // 			OSP Role Code       (8 - 9)
    // 			Service Provider Id (10 - 24)
    // 			Office Id           (25 - 39)
    // ·
    // ¼¼¼¼¼¼¼¼¼¼¼¼
    if (!IsEmpty(import.NextTranInfo.MiscText1))
    {
      if (Verify(Substring(import.NextTranInfo.MiscText1, 50, 1, 7),
        "0123456789") != 0)
      {
        export.NextTranInfo.MiscText1 =
          NumberToString(DateToJulianNumber(
            import.OfficeServiceProvider.EffectiveDate), 9, 7);
        export.NextTranInfo.MiscText1 =
          TrimEnd(export.NextTranInfo.MiscText1) + import
          .OfficeServiceProvider.RoleCode + NumberToString
          (import.ServiceProvider.SystemGeneratedId, 15) + NumberToString
          (import.Office.SystemGeneratedId, 15);

        return;
      }

      export.OfficeServiceProvider.EffectiveDate =
        StringToDate(Substring(import.NextTranInfo.MiscText1, 50, 1, 7));
      export.OfficeServiceProvider.RoleCode =
        Substring(import.NextTranInfo.MiscText1, 8, 2);
      export.ServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(import.NextTranInfo.MiscText1, 50, 10, 15));
        
      export.Office.SystemGeneratedId =
        (int)StringToNumber(Substring(import.NextTranInfo.MiscText1, 50, 25, 15));
        
    }
    else
    {
      export.NextTranInfo.MiscText1 =
        NumberToString(DateToJulianNumber(
          import.OfficeServiceProvider.EffectiveDate), 9, 7);
      export.NextTranInfo.MiscText1 = TrimEnd(export.NextTranInfo.MiscText1) + import
        .OfficeServiceProvider.RoleCode + NumberToString
        (import.ServiceProvider.SystemGeneratedId, 15) + NumberToString
        (import.Office.SystemGeneratedId, 15);
    }
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
