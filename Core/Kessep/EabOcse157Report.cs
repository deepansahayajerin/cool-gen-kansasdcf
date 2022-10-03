// Program: EAB_OCSE157_REPORT, ID: 372637266, model: 746.
// Short name: SWEXF710
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_OCSE157_REPORT.
/// </para>
/// <para>
/// Produce OCSE-157 report by sending data to Report Composer.
/// </para>
/// </summary>
[Serializable]
public partial class EabOcse157Report: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_OCSE157_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabOcse157Report(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabOcse157Report.
  /// </summary>
  public EabOcse157Report(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXF710", context, import, export, EabOptions.Hpvp);
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
    /// A value of Import1Current.
    /// </summary>
    [JsonPropertyName("import1Current")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1Current
    {
      get => import1Current ??= new();
      set => import1Current = value;
    }

    /// <summary>
    /// A value of Import1Former.
    /// </summary>
    [JsonPropertyName("import1Former")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1Former
    {
      get => import1Former ??= new();
      set => import1Former = value;
    }

    /// <summary>
    /// A value of Import1Never.
    /// </summary>
    [JsonPropertyName("import1Never")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1Never
    {
      get => import1Never ??= new();
      set => import1Never = value;
    }

    /// <summary>
    /// A value of Import1ACurrent.
    /// </summary>
    [JsonPropertyName("import1ACurrent")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1ACurrent
    {
      get => import1ACurrent ??= new();
      set => import1ACurrent = value;
    }

    /// <summary>
    /// A value of Import1AFormer.
    /// </summary>
    [JsonPropertyName("import1AFormer")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1AFormer
    {
      get => import1AFormer ??= new();
      set => import1AFormer = value;
    }

    /// <summary>
    /// A value of Import1ANever.
    /// </summary>
    [JsonPropertyName("import1ANever")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1ANever
    {
      get => import1ANever ??= new();
      set => import1ANever = value;
    }

    /// <summary>
    /// A value of Import1BCurrent.
    /// </summary>
    [JsonPropertyName("import1BCurrent")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1BCurrent
    {
      get => import1BCurrent ??= new();
      set => import1BCurrent = value;
    }

    /// <summary>
    /// A value of Import1BFormer.
    /// </summary>
    [JsonPropertyName("import1BFormer")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1BFormer
    {
      get => import1BFormer ??= new();
      set => import1BFormer = value;
    }

    /// <summary>
    /// A value of Import1BNever.
    /// </summary>
    [JsonPropertyName("import1BNever")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1BNever
    {
      get => import1BNever ??= new();
      set => import1BNever = value;
    }

    /// <summary>
    /// A value of Import1CNever.
    /// </summary>
    [JsonPropertyName("import1CNever")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Count" })]
    public Common Import1CNever
    {
      get => import1CNever ??= new();
      set => import1CNever = value;
    }

    /// <summary>
    /// A value of Import2Current.
    /// </summary>
    [JsonPropertyName("import2Current")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2Current
    {
      get => import2Current ??= new();
      set => import2Current = value;
    }

    /// <summary>
    /// A value of Import2Former.
    /// </summary>
    [JsonPropertyName("import2Former")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2Former
    {
      get => import2Former ??= new();
      set => import2Former = value;
    }

    /// <summary>
    /// A value of Import2Never.
    /// </summary>
    [JsonPropertyName("import2Never")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2Never
    {
      get => import2Never ??= new();
      set => import2Never = value;
    }

    /// <summary>
    /// A value of Import2ACurrent.
    /// </summary>
    [JsonPropertyName("import2ACurrent")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2ACurrent
    {
      get => import2ACurrent ??= new();
      set => import2ACurrent = value;
    }

    /// <summary>
    /// A value of Import2AFormer.
    /// </summary>
    [JsonPropertyName("import2AFormer")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2AFormer
    {
      get => import2AFormer ??= new();
      set => import2AFormer = value;
    }

    /// <summary>
    /// A value of Import2ANever.
    /// </summary>
    [JsonPropertyName("import2ANever")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2ANever
    {
      get => import2ANever ??= new();
      set => import2ANever = value;
    }

    /// <summary>
    /// A value of Import2BCurrent.
    /// </summary>
    [JsonPropertyName("import2BCurrent")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2BCurrent
    {
      get => import2BCurrent ??= new();
      set => import2BCurrent = value;
    }

    /// <summary>
    /// A value of Import2BFormer.
    /// </summary>
    [JsonPropertyName("import2BFormer")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2BFormer
    {
      get => import2BFormer ??= new();
      set => import2BFormer = value;
    }

    /// <summary>
    /// A value of Import2BNever.
    /// </summary>
    [JsonPropertyName("import2BNever")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2BNever
    {
      get => import2BNever ??= new();
      set => import2BNever = value;
    }

    /// <summary>
    /// A value of Import2CCurrent.
    /// </summary>
    [JsonPropertyName("import2CCurrent")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2CCurrent
    {
      get => import2CCurrent ??= new();
      set => import2CCurrent = value;
    }

    /// <summary>
    /// A value of Import2CFormer.
    /// </summary>
    [JsonPropertyName("import2CFormer")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2CFormer
    {
      get => import2CFormer ??= new();
      set => import2CFormer = value;
    }

    /// <summary>
    /// A value of Import2CNever.
    /// </summary>
    [JsonPropertyName("import2CNever")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2CNever
    {
      get => import2CNever ??= new();
      set => import2CNever = value;
    }

    /// <summary>
    /// A value of Import2DNever.
    /// </summary>
    [JsonPropertyName("import2DNever")]
    [Member(Index = 23, AccessFields = false, Members = new[] { "Count" })]
    public Common Import2DNever
    {
      get => import2DNever ??= new();
      set => import2DNever = value;
    }

    /// <summary>
    /// A value of Import3Current.
    /// </summary>
    [JsonPropertyName("import3Current")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Count" })]
    public Common Import3Current
    {
      get => import3Current ??= new();
      set => import3Current = value;
    }

    /// <summary>
    /// A value of Import3Former.
    /// </summary>
    [JsonPropertyName("import3Former")]
    [Member(Index = 25, AccessFields = false, Members = new[] { "Count" })]
    public Common Import3Former
    {
      get => import3Former ??= new();
      set => import3Former = value;
    }

    /// <summary>
    /// A value of Import3Never.
    /// </summary>
    [JsonPropertyName("import3Never")]
    [Member(Index = 26, AccessFields = false, Members = new[] { "Count" })]
    public Common Import3Never
    {
      get => import3Never ??= new();
      set => import3Never = value;
    }

    /// <summary>
    /// A value of Import4Total.
    /// </summary>
    [JsonPropertyName("import4Total")]
    [Member(Index = 27, AccessFields = false, Members = new[] { "Count" })]
    public Common Import4Total
    {
      get => import4Total ??= new();
      set => import4Total = value;
    }

    /// <summary>
    /// A value of Import5Total.
    /// </summary>
    [JsonPropertyName("import5Total")]
    [Member(Index = 28, AccessFields = false, Members = new[] { "Count" })]
    public Common Import5Total
    {
      get => import5Total ??= new();
      set => import5Total = value;
    }

    /// <summary>
    /// A value of Import6Total.
    /// </summary>
    [JsonPropertyName("import6Total")]
    [Member(Index = 29, AccessFields = false, Members = new[] { "Count" })]
    public Common Import6Total
    {
      get => import6Total ??= new();
      set => import6Total = value;
    }

    /// <summary>
    /// A value of Import7Total.
    /// </summary>
    [JsonPropertyName("import7Total")]
    [Member(Index = 30, AccessFields = false, Members = new[] { "Count" })]
    public Common Import7Total
    {
      get => import7Total ??= new();
      set => import7Total = value;
    }

    /// <summary>
    /// A value of Import8Total.
    /// </summary>
    [JsonPropertyName("import8Total")]
    [Member(Index = 31, AccessFields = false, Members = new[] { "Count" })]
    public Common Import8Total
    {
      get => import8Total ??= new();
      set => import8Total = value;
    }

    /// <summary>
    /// A value of Import9Total.
    /// </summary>
    [JsonPropertyName("import9Total")]
    [Member(Index = 32, AccessFields = false, Members = new[] { "Count" })]
    public Common Import9Total
    {
      get => import9Total ??= new();
      set => import9Total = value;
    }

    /// <summary>
    /// A value of Import10Total.
    /// </summary>
    [JsonPropertyName("import10Total")]
    [Member(Index = 33, AccessFields = false, Members = new[] { "Count" })]
    public Common Import10Total
    {
      get => import10Total ??= new();
      set => import10Total = value;
    }

    /// <summary>
    /// A value of Import12Current.
    /// </summary>
    [JsonPropertyName("import12Current")]
    [Member(Index = 34, AccessFields = false, Members = new[] { "Count" })]
    public Common Import12Current
    {
      get => import12Current ??= new();
      set => import12Current = value;
    }

    /// <summary>
    /// A value of Import12Former.
    /// </summary>
    [JsonPropertyName("import12Former")]
    [Member(Index = 35, AccessFields = false, Members = new[] { "Count" })]
    public Common Import12Former
    {
      get => import12Former ??= new();
      set => import12Former = value;
    }

    /// <summary>
    /// A value of Import12Never.
    /// </summary>
    [JsonPropertyName("import12Never")]
    [Member(Index = 36, AccessFields = false, Members = new[] { "Count" })]
    public Common Import12Never
    {
      get => import12Never ??= new();
      set => import12Never = value;
    }

    /// <summary>
    /// A value of Import13Current.
    /// </summary>
    [JsonPropertyName("import13Current")]
    [Member(Index = 37, AccessFields = false, Members = new[] { "Count" })]
    public Common Import13Current
    {
      get => import13Current ??= new();
      set => import13Current = value;
    }

    /// <summary>
    /// A value of Import13Former.
    /// </summary>
    [JsonPropertyName("import13Former")]
    [Member(Index = 38, AccessFields = false, Members = new[] { "Count" })]
    public Common Import13Former
    {
      get => import13Former ??= new();
      set => import13Former = value;
    }

    /// <summary>
    /// A value of Import13Never.
    /// </summary>
    [JsonPropertyName("import13Never")]
    [Member(Index = 39, AccessFields = false, Members = new[] { "Count" })]
    public Common Import13Never
    {
      get => import13Never ??= new();
      set => import13Never = value;
    }

    /// <summary>
    /// A value of Import14Total.
    /// </summary>
    [JsonPropertyName("import14Total")]
    [Member(Index = 40, AccessFields = false, Members = new[] { "Count" })]
    public Common Import14Total
    {
      get => import14Total ??= new();
      set => import14Total = value;
    }

    /// <summary>
    /// A value of Import16Current.
    /// </summary>
    [JsonPropertyName("import16Current")]
    [Member(Index = 41, AccessFields = false, Members = new[] { "Count" })]
    public Common Import16Current
    {
      get => import16Current ??= new();
      set => import16Current = value;
    }

    /// <summary>
    /// A value of Import16Former.
    /// </summary>
    [JsonPropertyName("import16Former")]
    [Member(Index = 42, AccessFields = false, Members = new[] { "Count" })]
    public Common Import16Former
    {
      get => import16Former ??= new();
      set => import16Former = value;
    }

    /// <summary>
    /// A value of Import16Never.
    /// </summary>
    [JsonPropertyName("import16Never")]
    [Member(Index = 43, AccessFields = false, Members = new[] { "Count" })]
    public Common Import16Never
    {
      get => import16Never ??= new();
      set => import16Never = value;
    }

    /// <summary>
    /// A value of Import17Current.
    /// </summary>
    [JsonPropertyName("import17Current")]
    [Member(Index = 44, AccessFields = false, Members = new[] { "Count" })]
    public Common Import17Current
    {
      get => import17Current ??= new();
      set => import17Current = value;
    }

    /// <summary>
    /// A value of Import17Former.
    /// </summary>
    [JsonPropertyName("import17Former")]
    [Member(Index = 45, AccessFields = false, Members = new[] { "Count" })]
    public Common Import17Former
    {
      get => import17Former ??= new();
      set => import17Former = value;
    }

    /// <summary>
    /// A value of Import17Never.
    /// </summary>
    [JsonPropertyName("import17Never")]
    [Member(Index = 46, AccessFields = false, Members = new[] { "Count" })]
    public Common Import17Never
    {
      get => import17Never ??= new();
      set => import17Never = value;
    }

    /// <summary>
    /// A value of Import18Current.
    /// </summary>
    [JsonPropertyName("import18Current")]
    [Member(Index = 47, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18Current
    {
      get => import18Current ??= new();
      set => import18Current = value;
    }

    /// <summary>
    /// A value of Import18Former.
    /// </summary>
    [JsonPropertyName("import18Former")]
    [Member(Index = 48, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18Former
    {
      get => import18Former ??= new();
      set => import18Former = value;
    }

    /// <summary>
    /// A value of Import18Never.
    /// </summary>
    [JsonPropertyName("import18Never")]
    [Member(Index = 49, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18Never
    {
      get => import18Never ??= new();
      set => import18Never = value;
    }

    /// <summary>
    /// A value of Import18ACurrent.
    /// </summary>
    [JsonPropertyName("import18ACurrent")]
    [Member(Index = 50, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18ACurrent
    {
      get => import18ACurrent ??= new();
      set => import18ACurrent = value;
    }

    /// <summary>
    /// A value of Import18AFormer.
    /// </summary>
    [JsonPropertyName("import18AFormer")]
    [Member(Index = 51, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18AFormer
    {
      get => import18AFormer ??= new();
      set => import18AFormer = value;
    }

    /// <summary>
    /// A value of Import18ANever.
    /// </summary>
    [JsonPropertyName("import18ANever")]
    [Member(Index = 52, AccessFields = false, Members = new[] { "Count" })]
    public Common Import18ANever
    {
      get => import18ANever ??= new();
      set => import18ANever = value;
    }

    /// <summary>
    /// A value of Import19Current.
    /// </summary>
    [JsonPropertyName("import19Current")]
    [Member(Index = 53, AccessFields = false, Members = new[] { "Count" })]
    public Common Import19Current
    {
      get => import19Current ??= new();
      set => import19Current = value;
    }

    /// <summary>
    /// A value of Import19Former.
    /// </summary>
    [JsonPropertyName("import19Former")]
    [Member(Index = 54, AccessFields = false, Members = new[] { "Count" })]
    public Common Import19Former
    {
      get => import19Former ??= new();
      set => import19Former = value;
    }

    /// <summary>
    /// A value of Import19Never.
    /// </summary>
    [JsonPropertyName("import19Never")]
    [Member(Index = 55, AccessFields = false, Members = new[] { "Count" })]
    public Common Import19Never
    {
      get => import19Never ??= new();
      set => import19Never = value;
    }

    /// <summary>
    /// A value of Import20Current.
    /// </summary>
    [JsonPropertyName("import20Current")]
    [Member(Index = 56, AccessFields = false, Members = new[] { "Count" })]
    public Common Import20Current
    {
      get => import20Current ??= new();
      set => import20Current = value;
    }

    /// <summary>
    /// A value of Import20Former.
    /// </summary>
    [JsonPropertyName("import20Former")]
    [Member(Index = 57, AccessFields = false, Members = new[] { "Count" })]
    public Common Import20Former
    {
      get => import20Former ??= new();
      set => import20Former = value;
    }

    /// <summary>
    /// A value of Import20Never.
    /// </summary>
    [JsonPropertyName("import20Never")]
    [Member(Index = 58, AccessFields = false, Members = new[] { "Count" })]
    public Common Import20Never
    {
      get => import20Never ??= new();
      set => import20Never = value;
    }

    /// <summary>
    /// A value of Import21Total.
    /// </summary>
    [JsonPropertyName("import21Total")]
    [Member(Index = 59, AccessFields = false, Members = new[] { "Count" })]
    public Common Import21Total
    {
      get => import21Total ??= new();
      set => import21Total = value;
    }

    /// <summary>
    /// A value of Import22Total.
    /// </summary>
    [JsonPropertyName("import22Total")]
    [Member(Index = 60, AccessFields = false, Members = new[] { "Count" })]
    public Common Import22Total
    {
      get => import22Total ??= new();
      set => import22Total = value;
    }

    /// <summary>
    /// A value of Import23Total.
    /// </summary>
    [JsonPropertyName("import23Total")]
    [Member(Index = 61, AccessFields = false, Members = new[] { "Count" })]
    public Common Import23Total
    {
      get => import23Total ??= new();
      set => import23Total = value;
    }

    /// <summary>
    /// A value of Import24Current.
    /// </summary>
    [JsonPropertyName("import24Current")]
    [Member(Index = 62, AccessFields = false, Members = new[] { "Count" })]
    public Common Import24Current
    {
      get => import24Current ??= new();
      set => import24Current = value;
    }

    /// <summary>
    /// A value of Import24Former.
    /// </summary>
    [JsonPropertyName("import24Former")]
    [Member(Index = 63, AccessFields = false, Members = new[] { "Count" })]
    public Common Import24Former
    {
      get => import24Former ??= new();
      set => import24Former = value;
    }

    /// <summary>
    /// A value of Import24Never.
    /// </summary>
    [JsonPropertyName("import24Never")]
    [Member(Index = 64, AccessFields = false, Members = new[] { "Count" })]
    public Common Import24Never
    {
      get => import24Never ??= new();
      set => import24Never = value;
    }

    /// <summary>
    /// A value of Import25Current.
    /// </summary>
    [JsonPropertyName("import25Current")]
    [Member(Index = 65, AccessFields = false, Members = new[] { "Count" })]
    public Common Import25Current
    {
      get => import25Current ??= new();
      set => import25Current = value;
    }

    /// <summary>
    /// A value of Import25Former.
    /// </summary>
    [JsonPropertyName("import25Former")]
    [Member(Index = 66, AccessFields = false, Members = new[] { "Count" })]
    public Common Import25Former
    {
      get => import25Former ??= new();
      set => import25Former = value;
    }

    /// <summary>
    /// A value of Import25Never.
    /// </summary>
    [JsonPropertyName("import25Never")]
    [Member(Index = 67, AccessFields = false, Members = new[] { "Count" })]
    public Common Import25Never
    {
      get => import25Never ??= new();
      set => import25Never = value;
    }

    /// <summary>
    /// A value of Import26Current.
    /// </summary>
    [JsonPropertyName("import26Current")]
    [Member(Index = 68, AccessFields = false, Members = new[] { "Count" })]
    public Common Import26Current
    {
      get => import26Current ??= new();
      set => import26Current = value;
    }

    /// <summary>
    /// A value of Import26Former.
    /// </summary>
    [JsonPropertyName("import26Former")]
    [Member(Index = 69, AccessFields = false, Members = new[] { "Count" })]
    public Common Import26Former
    {
      get => import26Former ??= new();
      set => import26Former = value;
    }

    /// <summary>
    /// A value of Import26Never.
    /// </summary>
    [JsonPropertyName("import26Never")]
    [Member(Index = 70, AccessFields = false, Members = new[] { "Count" })]
    public Common Import26Never
    {
      get => import26Never ??= new();
      set => import26Never = value;
    }

    /// <summary>
    /// A value of Import27Current.
    /// </summary>
    [JsonPropertyName("import27Current")]
    [Member(Index = 71, AccessFields = false, Members = new[] { "Count" })]
    public Common Import27Current
    {
      get => import27Current ??= new();
      set => import27Current = value;
    }

    /// <summary>
    /// A value of Import27Former.
    /// </summary>
    [JsonPropertyName("import27Former")]
    [Member(Index = 72, AccessFields = false, Members = new[] { "Count" })]
    public Common Import27Former
    {
      get => import27Former ??= new();
      set => import27Former = value;
    }

    /// <summary>
    /// A value of Import27Never.
    /// </summary>
    [JsonPropertyName("import27Never")]
    [Member(Index = 73, AccessFields = false, Members = new[] { "Count" })]
    public Common Import27Never
    {
      get => import27Never ??= new();
      set => import27Never = value;
    }

    /// <summary>
    /// A value of Import28Total.
    /// </summary>
    [JsonPropertyName("import28Total")]
    [Member(Index = 74, AccessFields = false, Members = new[] { "Count" })]
    public Common Import28Total
    {
      get => import28Total ??= new();
      set => import28Total = value;
    }

    /// <summary>
    /// A value of Import29Total.
    /// </summary>
    [JsonPropertyName("import29Total")]
    [Member(Index = 75, AccessFields = false, Members = new[] { "Count" })]
    public Common Import29Total
    {
      get => import29Total ??= new();
      set => import29Total = value;
    }

    /// <summary>
    /// A value of Import30Total.
    /// </summary>
    [JsonPropertyName("import30Total")]
    [Member(Index = 76, AccessFields = false, Members = new[] { "Count" })]
    public Common Import30Total
    {
      get => import30Total ??= new();
      set => import30Total = value;
    }

    /// <summary>
    /// A value of Import31Total.
    /// </summary>
    [JsonPropertyName("import31Total")]
    [Member(Index = 77, AccessFields = false, Members = new[] { "Count" })]
    public Common Import31Total
    {
      get => import31Total ??= new();
      set => import31Total = value;
    }

    /// <summary>
    /// A value of Import32Total.
    /// </summary>
    [JsonPropertyName("import32Total")]
    [Member(Index = 78, AccessFields = false, Members = new[] { "Count" })]
    public Common Import32Total
    {
      get => import32Total ??= new();
      set => import32Total = value;
    }

    /// <summary>
    /// A value of Import38Current.
    /// </summary>
    [JsonPropertyName("import38Current")]
    [Member(Index = 79, AccessFields = false, Members = new[] { "Count" })]
    public Common Import38Current
    {
      get => import38Current ??= new();
      set => import38Current = value;
    }

    /// <summary>
    /// A value of Import39Current.
    /// </summary>
    [JsonPropertyName("import39Current")]
    [Member(Index = 80, AccessFields = false, Members = new[] { "Count" })]
    public Common Import39Current
    {
      get => import39Current ??= new();
      set => import39Current = value;
    }

    /// <summary>
    /// A value of Import40Total.
    /// </summary>
    [JsonPropertyName("import40Total")]
    [Member(Index = 81, AccessFields = false, Members = new[] { "Count" })]
    public Common Import40Total
    {
      get => import40Total ??= new();
      set => import40Total = value;
    }

    /// <summary>
    /// A value of Import41Total.
    /// </summary>
    [JsonPropertyName("import41Total")]
    [Member(Index = 82, AccessFields = false, Members = new[] { "Count" })]
    public Common Import41Total
    {
      get => import41Total ??= new();
      set => import41Total = value;
    }

    /// <summary>
    /// A value of Import42Total.
    /// </summary>
    [JsonPropertyName("import42Total")]
    [Member(Index = 83, AccessFields = false, Members = new[] { "Count" })]
    public Common Import42Total
    {
      get => import42Total ??= new();
      set => import42Total = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 84, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private Common import1Current;
    private Common import1Former;
    private Common import1Never;
    private Common import1ACurrent;
    private Common import1AFormer;
    private Common import1ANever;
    private Common import1BCurrent;
    private Common import1BFormer;
    private Common import1BNever;
    private Common import1CNever;
    private Common import2Current;
    private Common import2Former;
    private Common import2Never;
    private Common import2ACurrent;
    private Common import2AFormer;
    private Common import2ANever;
    private Common import2BCurrent;
    private Common import2BFormer;
    private Common import2BNever;
    private Common import2CCurrent;
    private Common import2CFormer;
    private Common import2CNever;
    private Common import2DNever;
    private Common import3Current;
    private Common import3Former;
    private Common import3Never;
    private Common import4Total;
    private Common import5Total;
    private Common import6Total;
    private Common import7Total;
    private Common import8Total;
    private Common import9Total;
    private Common import10Total;
    private Common import12Current;
    private Common import12Former;
    private Common import12Never;
    private Common import13Current;
    private Common import13Former;
    private Common import13Never;
    private Common import14Total;
    private Common import16Current;
    private Common import16Former;
    private Common import16Never;
    private Common import17Current;
    private Common import17Former;
    private Common import17Never;
    private Common import18Current;
    private Common import18Former;
    private Common import18Never;
    private Common import18ACurrent;
    private Common import18AFormer;
    private Common import18ANever;
    private Common import19Current;
    private Common import19Former;
    private Common import19Never;
    private Common import20Current;
    private Common import20Former;
    private Common import20Never;
    private Common import21Total;
    private Common import22Total;
    private Common import23Total;
    private Common import24Current;
    private Common import24Former;
    private Common import24Never;
    private Common import25Current;
    private Common import25Former;
    private Common import25Never;
    private Common import26Current;
    private Common import26Former;
    private Common import26Never;
    private Common import27Current;
    private Common import27Former;
    private Common import27Never;
    private Common import28Total;
    private Common import29Total;
    private Common import30Total;
    private Common import31Total;
    private Common import32Total;
    private Common import38Current;
    private Common import39Current;
    private Common import40Total;
    private Common import41Total;
    private Common import42Total;
    private ReportParms reportParms;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
