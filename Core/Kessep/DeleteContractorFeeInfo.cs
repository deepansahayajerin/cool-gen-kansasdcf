// Program: DELETE_CONTRACTOR_FEE_INFO, ID: 371803717, model: 746.
// Short name: SWE00202
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: DELETE_CONTRACTOR_FEE_INFO.
/// </summary>
[Serializable]
public partial class DeleteContractorFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_CONTRACTOR_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteContractorFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteContractorFeeInfo.
  /// </summary>
  public DeleteContractorFeeInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************************
    // This action block will use the information passed into it to DELETE
    // a CONTRACTOR FEE INFORMATION record.
    // ************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadContractorFeeInformation())
    {
      DeleteContractorFeeInformation();
    }
    else
    {
      ExitState = "FN0000_COLL_FOR_SEL_REC_NF";
    }
  }

  private void DeleteContractorFeeInformation()
  {
    Update("DeleteContractorFeeInformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier",
          entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "offId", entities.ExistingContractorFeeInformation.OffId);
      });
  }

  private bool ReadContractorFeeInformation()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier",
          import.ContractorFeeInformation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 1);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
    }

    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("existingContractorFeeInformation")]
    public ContractorFeeInformation ExistingContractorFeeInformation
    {
      get => existingContractorFeeInformation ??= new();
      set => existingContractorFeeInformation = value;
    }

    private ObligationType existingObligationType;
    private ContractorFeeInformation existingContractorFeeInformation;
  }
#endregion
}
