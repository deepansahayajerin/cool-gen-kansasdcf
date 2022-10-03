// Program: LE_SERV_VALIDATE_SERVICE_PROCESS, ID: 372015475, model: 746.
// Short name: SWE00819
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
/// A program: LE_SERV_VALIDATE_SERVICE_PROCESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block validates Service Process information.
/// </para>
/// </summary>
[Serializable]
public partial class LeServValidateServiceProcess: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SERV_VALIDATE_SERVICE_PROCESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeServValidateServiceProcess(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeServValidateServiceProcess.
  /// </summary>
  public LeServValidateServiceProcess(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR #	Description
    // ?????	govind		Initial Code
    // 053097	govind		Removed the edits for mandatory Service Date and Servee on
    // 				 Return of Service
    // 08/29/97 JF.Caillouet	Removed Test for Service_name 'error code #9'
    // 11/18/98 R.Jean         Removed LEGAL ACTION read;
    // remove SERVICE PROCESS read; change current usage
    // ---------------------------------------------
    export.ErrorCodes.Index = -1;

    if (Equal(import.ServiceProcess.ServiceRequestDate,
      local.InitialisedToSpaces.ServiceRequestDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 4;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Lt(Now().Date, import.ServiceProcess.ServiceRequestDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 11;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    local.Code.CodeName = "METHOD OF SERVICE";
    local.CodeValue.Cdvalue = import.ServiceProcess.MethodOfService;
    UseCabValidateCodeValue();

    if (AsChar(local.ValidCode.Flag) == 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 14;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    local.Code.CodeName = "SERVICE PROCESS DOCUMENT";
    local.CodeValue.Cdvalue = import.ServiceProcess.ServiceDocumentType;
    UseCabValidateCodeValue();

    if (AsChar(local.ValidCode.Flag) == 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 5;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.ServiceProcess.RequestedServee))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 6;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    // ---------------------------------------------
    // mandatory field check on service location : error code 7 has been removed
    // from here
    // ---------------------------------------------
    // ---------------------------------------------
    // Validate the service result details if any of the result detail is 
    // supplied.
    // ---------------------------------------------
    if (Lt(local.InitialisedToSpaces.ServiceRequestDate,
      import.ServiceProcess.ServiceDate) || Lt
      (local.InitialisedToSpaces.ReturnDate, import.ServiceProcess.ReturnDate) ||
      !IsEmpty(import.ServiceProcess.ServerName) || !
      IsEmpty(import.ServiceProcess.Servee) || !
      IsEmpty(import.ServiceProcess.ServeeRelationship) || !
      IsEmpty(import.ServiceProcess.ServiceResult))
    {
      // ---------------------------------------------
      // Removed the check for mandatory Service Date when the results are 
      // entered. Service may not always be successful. Error code 8 is no
      // longer valid
      // ---------------------------------------------
      if (Lt(Now().Date, import.ServiceProcess.ServiceDate) || Lt
        (local.InitialisedToSpaces.ServiceDate,
        import.ServiceProcess.ServiceDate) && Lt
        (import.ServiceProcess.ServiceDate,
        import.ServiceProcess.ServiceRequestDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 12;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (Lt(Now().Date, import.ServiceProcess.ReturnDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 13;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (Lt(import.ServiceProcess.ReturnDate, import.ServiceProcess.ServiceDate)
        || Lt
        (import.ServiceProcess.ReturnDate,
        import.ServiceProcess.ServiceRequestDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 13;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      // ---------------------------------------------
      // The check for mandatory Server_Name has been removed from here. Error 
      // code 9 is no longer valid.
      // ---------------------------------------------
      // ---------------------------------------------
      // The check for mandatory Servee has been removed from here. Error code 
      // 10 is no longer valid.
      // ---------------------------------------------
    }
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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

    private Common userAction;
    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public ServiceProcess InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private ServiceProcess initialisedToSpaces;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProcess.
    /// </summary>
    [JsonPropertyName("existingServiceProcess")]
    public ServiceProcess ExistingServiceProcess
    {
      get => existingServiceProcess ??= new();
      set => existingServiceProcess = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private ServiceProcess existingServiceProcess;
    private LegalAction existingLegalAction;
  }
#endregion
}
