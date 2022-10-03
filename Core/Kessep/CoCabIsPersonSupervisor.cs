// Program: CO_CAB_IS_PERSON_SUPERVISOR, ID: 371748106, model: 746.
// Short name: SWE01957
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CO_CAB_IS_PERSON_SUPERVISOR.
/// </para>
/// <para>
/// Input : Userid (Top Secret Userid)
///         Date - Indicates date used to test for active roles. This is 
/// provided so this can be used in batch too, if needed.
/// Output : Flag, Y if person is supervisor
///                N if person is not supervisor
/// </para>
/// </summary>
[Serializable]
public partial class CoCabIsPersonSupervisor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CAB_IS_PERSON_SUPERVISOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCabIsPersonSupervisor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCabIsPersonSupervisor.
  /// </summary>
  public CoCabIsPersonSupervisor(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Date		Developer	Request #      Description ø
    // øææææææææææææææææææææææææææææææææææææææææææææææææææææææææææø
    // ø15 Apr 97      Siraj Konkader              Initial Dev    ø
    // ø
    // 
    // ø
    // ø
    // 
    // ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // ø                    processing explanation                ø
    // ø 1.0 Read each Active Office Service Provider and xref    ø
    // ø their Role Codes against the Code Value Table, Code Name ø
    // ø 'Supervisor Role Codes'
    // 
    // ø
    // ø
    // 
    // ø
    // ø If person is of Supervisor Level, return Y else return N ø
    // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    export.IsSupervisor.Flag = "N";
    local.Code.CodeName = "SUPERVISOR ROLE CODES";

    foreach(var item in ReadOfficeServiceProvider())
    {
      local.CodeValue.Cdvalue = entities.OfficeServiceProvider.RoleCode;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.IsSupervisor.Flag = "Y";

        return;
      }
    }
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private IEnumerable<bool> ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", import.ServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDtOrCurrentDt.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;

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
    /// A value of ProcessDtOrCurrentDt.
    /// </summary>
    [JsonPropertyName("processDtOrCurrentDt")]
    public DateWorkArea ProcessDtOrCurrentDt
    {
      get => processDtOrCurrentDt ??= new();
      set => processDtOrCurrentDt = value;
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

    private DateWorkArea processDtOrCurrentDt;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    private Common isSupervisor;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private Common validCode;
    private Code code;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
