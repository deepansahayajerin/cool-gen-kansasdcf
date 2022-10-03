// Program: FN_PROTECT_COLLECTIONS_FOR_OBLI2, ID: 373381724, model: 746.
// Short name: SWE02099
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROTECT_COLLECTIONS_FOR_OBLI2.
/// </summary>
[Serializable]
public partial class FnProtectCollectionsForObli2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROTECT_COLLECTIONS_FOR_OBLI2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProtectCollectionsForObli2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProtectCollectionsForObli2.
  /// </summary>
  public FnProtectCollectionsForObli2(IContext context, Import import,
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
    local.CreateObCollProtHist.Flag = import.CreateObCollProtHist.Flag;
    export.CollsFndToProtect.Flag = "N";
    export.ObCollProtHistCreated.Flag = "N";

    if (AsChar(import.Persistent.PrimarySecondaryCode) == 'S')
    {
      local.CreateObCollProtHist.Flag = "N";
    }
    else if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
    {
      if (ReadObligCollProtectionHist1())
      {
        local.CreateObCollProtHist.Flag = "N";
      }
      else
      {
        local.CreateObCollProtHist.Flag = "Y";
      }
    }
    else if (ReadObligCollProtectionHist2())
    {
      local.CreateObCollProtHist.Flag = "N";
    }
    else
    {
      local.CreateObCollProtHist.Flag = "Y";
    }

    if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
    {
      foreach(var item in ReadCollection2())
      {
        export.CollsFndToProtect.Flag = "Y";

        try
        {
          UpdateCollection();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_COLLECTION_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      foreach(var item in ReadCollection1())
      {
        export.CollsFndToProtect.Flag = "Y";

        try
        {
          UpdateCollection();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_COLLECTION_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    if (AsChar(export.CollsFndToProtect.Flag) == 'N')
    {
      return;
    }

    if (AsChar(local.CreateObCollProtHist.Flag) == 'N')
    {
      return;
    }

    if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
    {
      foreach(var item in ReadObligCollProtectionHist5())
      {
        if (Lt(entities.ExistingObligCollProtectionHist.CvrdCollStrtDt,
          import.ObligCollProtectionHist.CvrdCollStrtDt))
        {
          try
          {
            UpdateObligCollProtectionHist();

            try
            {
              CreateObligCollProtectionHist3();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (Lt(import.ObligCollProtectionHist.CvrdCollEndDt,
          entities.ExistingObligCollProtectionHist.CvrdCollEndDt))
        {
          try
          {
            UpdateObligCollProtectionHist();

            try
            {
              CreateObligCollProtectionHist1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          try
          {
            UpdateObligCollProtectionHist();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else
    {
      foreach(var item in ReadObligCollProtectionHist3())
      {
        if (Lt(import.ObligCollProtectionHist.CvrdCollStrtDt,
          entities.ExistingObligCollProtectionHist.CvrdCollStrtDt))
        {
          try
          {
            UpdateObligCollProtectionHist();

            try
            {
              CreateObligCollProtectionHist4();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (Lt(entities.ExistingObligCollProtectionHist.CvrdCollEndDt,
          import.ObligCollProtectionHist.CvrdCollEndDt))
        {
          try
          {
            UpdateObligCollProtectionHist();

            try
            {
              CreateObligCollProtectionHist2();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          try
          {
            UpdateObligCollProtectionHist();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      local.OverrideCollProtFnd.Flag = "N";

      foreach(var item in ReadObligCollProtectionHist4())
      {
        local.OverrideCollProtFnd.Flag = "Y";

        if (!Lt(import.ObligCollProtectionHist.CvrdCollStrtDt,
          entities.ExistingObligCollProtectionHist.CvrdCollStrtDt))
        {
          local.Tmp.CvrdCollStrtDt =
            AddDays(entities.ExistingObligCollProtectionHist.CvrdCollEndDt, 1);
          local.Tmp.CvrdCollEndDt =
            import.ObligCollProtectionHist.CvrdCollEndDt;
        }
        else if (Lt(import.ObligCollProtectionHist.CvrdCollStrtDt,
          entities.ExistingObligCollProtectionHist.CvrdCollStrtDt) && !
          Lt(import.ObligCollProtectionHist.CvrdCollEndDt,
          entities.ExistingObligCollProtectionHist.CvrdCollEndDt))
        {
          if (Equal(local.Tmp.CvrdCollStrtDt, local.NullDateWorkArea.Date))
          {
            local.Tmp.CvrdCollStrtDt =
              import.ObligCollProtectionHist.CvrdCollStrtDt;
          }

          local.Tmp.CvrdCollEndDt =
            AddDays(entities.ExistingObligCollProtectionHist.CvrdCollStrtDt, -1);
            

          try
          {
            CreateObligCollProtectionHist6();
            export.ObCollProtHistCreated.Flag = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.Tmp.CvrdCollStrtDt = AddDays(local.Tmp.CvrdCollEndDt, 1);
          local.Tmp.CvrdCollEndDt =
            import.ObligCollProtectionHist.CvrdCollEndDt;
        }
        else
        {
          if (Equal(local.Tmp.CvrdCollStrtDt, local.NullDateWorkArea.Date))
          {
            local.Tmp.CvrdCollStrtDt =
              import.ObligCollProtectionHist.CvrdCollStrtDt;
          }

          local.Tmp.CvrdCollEndDt =
            AddDays(entities.ExistingObligCollProtectionHist.CvrdCollStrtDt, -1);
            

          try
          {
            CreateObligCollProtectionHist6();
            export.ObCollProtHistCreated.Flag = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          MoveObligCollProtectionHist(local.NullObligCollProtectionHist,
            local.Tmp);
        }
      }

      if (AsChar(local.OverrideCollProtFnd.Flag) == 'N')
      {
        goto Test;
      }

      if (!Equal(local.Tmp.CvrdCollStrtDt, local.NullDateWorkArea.Date))
      {
        try
        {
          CreateObligCollProtectionHist6();
          export.ObCollProtHistCreated.Flag = "Y";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      return;
    }

Test:

    try
    {
      CreateObligCollProtectionHist5();
      export.ObCollProtHistCreated.Flag = "Y";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveObligCollProtectionHist(
    ObligCollProtectionHist source, ObligCollProtectionHist target)
  {
    target.CvrdCollStrtDt = source.CvrdCollStrtDt;
    target.CvrdCollEndDt = source.CvrdCollEndDt;
  }

  private void CreateObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt =
      AddDays(import.ObligCollProtectionHist.CvrdCollEndDt, 1);
    var cvrdCollEndDt = entities.ExistingObligCollProtectionHist.CvrdCollEndDt;
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = entities.ExistingObligCollProtectionHist.ReasonText;
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel =
      entities.ExistingObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private void CreateObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt =
      AddDays(import.ObligCollProtectionHist.CvrdCollEndDt, 1);
    var cvrdCollEndDt = entities.ExistingObligCollProtectionHist.CvrdCollEndDt;
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = entities.ExistingObligCollProtectionHist.LastUpdatedBy ?? Spaces
      (240);
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel = import.ObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private void CreateObligCollProtectionHist3()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt =
      entities.ExistingObligCollProtectionHist.CvrdCollStrtDt;
    var cvrdCollEndDt =
      AddDays(import.ObligCollProtectionHist.CvrdCollStrtDt, -1);
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = entities.ExistingObligCollProtectionHist.ReasonText;
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel =
      entities.ExistingObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist3",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private void CreateObligCollProtectionHist4()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt =
      entities.ExistingObligCollProtectionHist.CvrdCollStrtDt;
    var cvrdCollEndDt =
      AddDays(import.ObligCollProtectionHist.CvrdCollStrtDt, -1);
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = entities.ExistingObligCollProtectionHist.LastUpdatedBy ?? Spaces
      (240);
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel = import.ObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist4",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private void CreateObligCollProtectionHist5()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt = import.ObligCollProtectionHist.CvrdCollStrtDt;
    var cvrdCollEndDt = import.ObligCollProtectionHist.CvrdCollEndDt;
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = import.ObligCollProtectionHist.ReasonText;
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel = import.ObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist5",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private void CreateObligCollProtectionHist6()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var cvrdCollStrtDt = local.Tmp.CvrdCollStrtDt;
    var cvrdCollEndDt = local.Tmp.CvrdCollEndDt;
    var deactivationDate = local.NullDateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = import.ObligCollProtectionHist.ReasonText;
    var cspNumber = import.Persistent.CspNumber;
    var cpaType = import.Persistent.CpaType;
    var otyIdentifier = import.Persistent.DtyGeneratedId;
    var obgIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var protectionLevel = import.ObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligCollProtectionHist6",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.New1.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.New1.CvrdCollEndDt = cvrdCollEndDt;
    entities.New1.DeactivationDate = deactivationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.ReasonText = reasonText;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.ProtectionLevel = protectionLevel;
    entities.New1.Populated = true;
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 4);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 7);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 8);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 9);
        entities.ExistingCollection.CpaType = db.GetString(reader, 10);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 11);
        entities.ExistingCollection.OtrType = db.GetString(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 4);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 7);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 8);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 9);
        entities.ExistingCollection.CpaType = db.GetString(reader, 10);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 11);
        entities.ExistingCollection.OtrType = db.GetString(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;

        return true;
      });
  }

  private bool ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyIdentifier", import.Persistent.DtyGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingObligCollProtectionHist.ReasonText =
          db.GetString(reader, 6);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 11);
        entities.ExistingObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyIdentifier", import.Persistent.DtyGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetString(
          command, "protectionLevel",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingObligCollProtectionHist.ReasonText =
          db.GetString(reader, 6);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 11);
        entities.ExistingObligCollProtectionHist.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist3()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyIdentifier", import.Persistent.DtyGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetString(
          command, "protectionLevel",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingObligCollProtectionHist.ReasonText =
          db.GetString(reader, 6);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 11);
        entities.ExistingObligCollProtectionHist.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist4()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist4",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyIdentifier", import.Persistent.DtyGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingObligCollProtectionHist.ReasonText =
          db.GetString(reader, 6);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 11);
        entities.ExistingObligCollProtectionHist.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist5()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist5",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyIdentifier", import.Persistent.DtyGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingObligCollProtectionHist.ReasonText =
          db.GetString(reader, 6);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 11);
        entities.ExistingObligCollProtectionHist.Populated = true;

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = "P";

    CheckValid<Collection>("DistributionMethod", distributionMethod);
    entities.ExistingCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetInt32(
          command, "collId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.ExistingCollection.CrtType);
        db.SetInt32(command, "cstId", entities.ExistingCollection.CstId);
        db.SetInt32(command, "crvId", entities.ExistingCollection.CrvId);
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.SetInt32(command, "obgId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.SetInt32(command, "otrId", entities.ExistingCollection.OtrId);
        db.SetString(command, "otrType", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "otyId", entities.ExistingCollection.OtyId);
      });

    entities.ExistingCollection.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCollection.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCollection.DistributionMethod = distributionMethod;
    entities.ExistingCollection.Populated = true;
  }

  private void UpdateObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligCollProtectionHist.Populated);

    var deactivationDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ExistingObligCollProtectionHist.Populated = false;
    Update("UpdateObligCollProtectionHist",
      (db, command) =>
      {
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetDateTime(
          command, "createdTmst",
          entities.ExistingObligCollProtectionHist.CreatedTmst.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligCollProtectionHist.CspNumber);
        db.SetString(
          command, "cpaType", entities.ExistingObligCollProtectionHist.CpaType);
          
        db.SetInt32(
          command, "otyIdentifier",
          entities.ExistingObligCollProtectionHist.OtyIdentifier);
        db.SetInt32(
          command, "obgIdentifier",
          entities.ExistingObligCollProtectionHist.ObgIdentifier);
      });

    entities.ExistingObligCollProtectionHist.DeactivationDate =
      deactivationDate;
    entities.ExistingObligCollProtectionHist.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingObligCollProtectionHist.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingObligCollProtectionHist.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Obligation Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
    }

    private Obligation persistent;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common createObCollProtHist;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
    }

    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    private Common collsFndToProtect;
    private Common obCollProtHistCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of OverrideCollProtFnd.
    /// </summary>
    [JsonPropertyName("overrideCollProtFnd")]
    public Common OverrideCollProtFnd
    {
      get => overrideCollProtFnd ??= new();
      set => overrideCollProtFnd = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public ObligCollProtectionHist Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of NullObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("nullObligCollProtectionHist")]
    public ObligCollProtectionHist NullObligCollProtectionHist
    {
      get => nullObligCollProtectionHist ??= new();
      set => nullObligCollProtectionHist = value;
    }

    private Common createObCollProtHist;
    private DateWorkArea nullDateWorkArea;
    private Common overrideCollProtFnd;
    private ObligCollProtectionHist tmp;
    private ObligCollProtectionHist nullObligCollProtectionHist;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("existingObligCollProtectionHist")]
    public ObligCollProtectionHist ExistingObligCollProtectionHist
    {
      get => existingObligCollProtectionHist ??= new();
      set => existingObligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligCollProtectionHist New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private ObligCollProtectionHist existingObligCollProtectionHist;
    private Collection existingCollection;
    private ObligationTransaction existingDebt;
    private ObligCollProtectionHist new1;
  }
#endregion
}
