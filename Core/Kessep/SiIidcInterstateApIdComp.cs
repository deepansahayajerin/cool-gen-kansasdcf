// Program: SI_IIDC_INTERSTATE_AP_ID_COMP, ID: 372515348, model: 746.
// Short name: SWEIIDCP
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
/// A program: SI_IIDC_INTERSTATE_AP_ID_COMP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIidcInterstateApIdComp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIDC_INTERSTATE_AP_ID_COMP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIidcInterstateApIdComp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIidcInterstateApIdComp.
  /// </summary>
  public SiIidcInterstateApIdComp(IContext context, Import import, Export export)
    :
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
    // 05-22-95  Ken Evans - MTW	Initial development
    // 02-01-96  J. Howard - SRS	Retrofit
    // 11/04/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 4/13/99    C. Ott               Added Interstate Case Transaction
    //                                 
    // Date to fully qualify reads of
    //                                 
    // that entity.
    // 2/01/00    C. Scroggins         Added reads to determine whether
    //                                 
    // there was more than one AP
    //                                 
    // associated to the case and flow
    // to
    //                                 
    // COMP.
    // ------------------------------------------------------------
    // ***************************************************************
    // 8/25/99  C. Ott   Added check for Person 'owned by' AE. Certain data may 
    // not be updated on these records.
    // ***************************************************************
    // 09/05/07   J HARDEN   PR192355   Changeing IIDC screen to save
    // alt SSN
    // 
    // on the ALTS screen
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.OwnedByAe.Flag = import.OwnedByAe.Flag;
    export.Csenet.Assign(import.Csenet);
    export.InterstateCase.Assign(import.InterstateCase);
    export.CseCsePerson.Assign(import.CseCsePerson);
    export.CseCsePersonsWorkSet.Assign(import.CseCsePersonsWorkSet);
    export.CsePersonLicense.Assign(import.CsePersonLicense);
    export.Export1St.Assign(import.Import1St);
    export.Export2Nd.Assign(import.Import2Nd);
    export.AltSsn1.SelectChar = import.AltSsn1.SelectChar;
    export.AltSsn2.SelectChar = import.AltSsn2.SelectChar;
    export.Dob.SelectChar = import.Dob.SelectChar;
    export.EyeCol.SelectChar = import.EyeCol.SelectChar;
    export.Fn.SelectChar = import.Fn.SelectChar;
    export.HairCol.SelectChar = import.HairCol.SelectChar;
    export.Height.SelectChar = import.Height.SelectChar;
    export.Ln.SelectChar = import.Ln.SelectChar;
    export.Mn.SelectChar = import.Mn.SelectChar;
    export.OtherId.SelectChar = import.OtherId.SelectChar;
    export.Pob.SelectChar = import.Pob.SelectChar;
    export.Race.SelectChar = import.Race.SelectChar;
    export.Sex.SelectChar = import.Sex.SelectChar;
    export.Ssn.SelectChar = import.Ssn.SelectChar;
    export.Weight.SelectChar = import.Weight.SelectChar;

    if (AsChar(export.OwnedByAe.Flag) == 'O')
    {
      var field1 = GetField(export.Ln, "selectChar");

      field1.Protected = true;

      var field2 = GetField(export.Fn, "selectChar");

      field2.Protected = true;

      var field3 = GetField(export.Mn, "selectChar");

      field3.Protected = true;

      var field4 = GetField(export.Dob, "selectChar");

      field4.Protected = true;

      var field5 = GetField(export.Sex, "selectChar");

      field5.Protected = true;

      var field6 = GetField(export.Ssn, "selectChar");

      field6.Protected = true;
    }

    if (!import.PartList.IsEmpty)
    {
      export.PartList.Index = -1;

      for(import.PartList.Index = 0; import.PartList.Index < import
        .PartList.Count; ++import.PartList.Index)
      {
        if (!import.PartList.CheckSize())
        {
          break;
        }

        ++export.PartList.Index;
        export.PartList.CheckSize();

        export.PartList.Update.G.Assign(import.PartList.Item.G);
      }

      import.PartList.CheckIndex();
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

    if (Equal(global.Command, "XXFMMENU") || Equal(global.Command, "RETCOMP"))
    {
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "COMP":
        break;
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
      case "DISPLAY":
        export.AltSsn1.SelectChar = "";
        export.AltSsn2.SelectChar = "";
        export.Dob.SelectChar = "";
        export.EyeCol.SelectChar = "";
        export.Fn.SelectChar = "";
        export.HairCol.SelectChar = "";
        export.Height.SelectChar = "";
        export.Ln.SelectChar = "";
        export.Mn.SelectChar = "";
        export.OtherId.SelectChar = "";
        export.Pob.SelectChar = "";
        export.Race.SelectChar = "";
        export.Sex.SelectChar = "";
        export.Ssn.SelectChar = "";
        export.Weight.SelectChar = "";
        UseSiReadCsenetApIdData();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Csenet, "nameLast");

          field.Error = true;

          return;
        }

        UseSiRetrieveApIdCompareInfo();

        if (IsExitState("NUMBER_OF_APS_EXCEED_MAX"))
        {
          export.Case1.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
          export.CseCsePersonsWorkSet.LastName = "";
          export.CseCsePersonsWorkSet.FirstName = "";
          export.CseCsePersonsWorkSet.MiddleInitial = "";
          export.CseCsePersonsWorkSet.Number = "";
          export.CseCsePersonsWorkSet.Sex = "";
          export.CseCsePersonsWorkSet.Ssn = "";
          ExitState = "SI0000_MULTIPLE_APS_EXIST_FOR_CS";

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          // ***************************************************************
          // 8/25/99  C. Ott   Added check for Person 'owned by' AE.  If this 
          // flag contains a value of 'O', certain data may not be updated.
          // ***************************************************************
          if (AsChar(export.OwnedByAe.Flag) == 'O')
          {
            var field1 = GetField(export.Ln, "selectChar");

            field1.Protected = true;

            var field2 = GetField(export.Fn, "selectChar");

            field2.Protected = true;

            var field3 = GetField(export.Mn, "selectChar");

            field3.Protected = true;

            var field4 = GetField(export.Dob, "selectChar");

            field4.Protected = true;

            var field5 = GetField(export.Sex, "selectChar");

            field5.Protected = true;

            var field6 = GetField(export.Ssn, "selectChar");

            field6.Protected = true;
          }
        }

        if (StringToNumber(export.Csenet.Ssn) == 0)
        {
          export.Csenet.Ssn = "";
        }

        if (StringToNumber(export.Csenet.AliasSsn1) == 0)
        {
          export.Csenet.AliasSsn1 = "";
        }

        if (StringToNumber(export.Csenet.AliasSsn2) == 0)
        {
          export.Csenet.AliasSsn2 = "";
        }

        if (StringToNumber(export.CseCsePersonsWorkSet.Ssn) == 0)
        {
          export.CseCsePersonsWorkSet.Ssn = "";
        }

        if (StringToNumber(export.Export1St.Ssn) == 0)
        {
          export.Export1St.Ssn = "";
        }

        if (StringToNumber(export.Export2Nd.Ssn) == 0)
        {
          export.Export2Nd.Ssn = "";
        }

        break;
      case "UPDATE":
        local.Local2Nd.Assign(export.Export2Nd);
        local.Local1St.Assign(export.Export1St);
        local.CseCsePerson.Assign(export.CseCsePerson);
        local.CseCsePersonsWorkSet.Assign(export.CseCsePersonsWorkSet);

        // ***************************************************************
        // 8/25/99  C. Ott   Added check for Person 'owned by' AE.  If this flag
        // contains a value of 'O', certain Person data may not be updated.
        // ***************************************************************
        if (AsChar(export.OwnedByAe.Flag) == 'O')
        {
          var field1 = GetField(export.Ln, "selectChar");

          field1.Protected = true;

          var field2 = GetField(export.Fn, "selectChar");

          field2.Protected = true;

          var field3 = GetField(export.Mn, "selectChar");

          field3.Protected = true;

          var field4 = GetField(export.Dob, "selectChar");

          field4.Protected = true;

          var field5 = GetField(export.Sex, "selectChar");

          field5.Protected = true;

          var field6 = GetField(export.Ssn, "selectChar");

          field6.Protected = true;

          if (!IsEmpty(import.Ln.SelectChar) || !
            IsEmpty(import.Fn.SelectChar) || !IsEmpty(import.Mn.SelectChar) || !
            IsEmpty(import.Dob.SelectChar) || !
            IsEmpty(import.Sex.SelectChar) || !IsEmpty(import.Ssn.SelectChar))
          {
            ExitState = "CO0000_PERSON_NOT_UPDATEABLE";

            return;
          }
        }

        switch(AsChar(import.AltSsn2.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.AliasSsn2))
            {
              if (Equal(export.Csenet.AliasSsn2, export.Export2Nd.Ssn))
              {
                var field1 = GetField(export.Csenet, "aliasSsn2");

                field1.Color = "red";
                field1.Protected = true;

                var field2 = GetField(export.Export2Nd, "ssn");

                field2.Color = "red";
                field2.Protected = true;

                var field3 = GetField(export.AltSsn2, "selectChar");

                field3.Error = true;

                ExitState = "INVALID_UPDATE";

                return;
              }

              if (IsEmpty(export.CseCsePersonsWorkSet.Sex) || Equal
                (export.CseCsePersonsWorkSet.Dob, new DateTime(1, 1, 1)))
              {
                var field1 = GetField(export.AltSsn2, "selectChar");

                field1.Error = true;

                ExitState = "MISSING_SEX_OR_DOB";

                return;
              }

              if (IsEmpty(export.Export2Nd.Ssn))
              {
                local.UpdAlias2.Flag = "C";
              }
              else
              {
                // PR192355 change "U" to "C"
                local.UpdAlias2.Flag = "C";
              }

              local.Local2Nd.Ssn = export.Csenet.AliasSsn2 ?? Spaces(9);
            }
            else
            {
              var field1 = GetField(export.AltSsn2, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "aliasSsn2");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.AltSsn2, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.AltSsn1.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.AliasSsn1))
            {
              if (Equal(export.Csenet.AliasSsn1, export.Export1St.Ssn))
              {
                var field1 = GetField(export.Csenet, "aliasSsn1");

                field1.Color = "red";
                field1.Protected = true;

                var field2 = GetField(export.Export1St, "ssn");

                field2.Color = "red";
                field2.Protected = true;

                var field3 = GetField(export.AltSsn1, "selectChar");

                field3.Error = true;

                ExitState = "INVALID_UPDATE";

                return;
              }

              if (IsEmpty(export.CseCsePersonsWorkSet.Sex) || Equal
                (export.CseCsePersonsWorkSet.Dob, new DateTime(1, 1, 1)))
              {
                var field1 = GetField(export.AltSsn1, "selectChar");

                field1.Error = true;

                ExitState = "MISSING_SEX_OR_DOB";

                return;
              }

              if (IsEmpty(export.Export1St.Ssn))
              {
                local.UpdAlias1.Flag = "C";
              }
              else
              {
                // PR192355  changed "U" to "C"
                local.UpdAlias1.Flag = "C";
              }

              local.Local1St.Ssn = export.Csenet.AliasSsn1 ?? Spaces(9);
            }
            else
            {
              var field1 = GetField(export.AltSsn1, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "aliasSsn1");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.AltSsn1, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.OtherId.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.OtherIdInfo))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.OtherIdInfo = export.Csenet.OtherIdInfo ?? "";
            }
            else
            {
              var field1 = GetField(export.OtherId, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "otherIdInfo");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.OtherId, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Weight.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.Csenet.Weight.GetValueOrDefault() > 0)
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.Weight =
                export.Csenet.Weight.GetValueOrDefault();
            }
            else
            {
              var field1 = GetField(export.Weight, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "weight");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Weight, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.HairCol.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.HairColor))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.HairColor = export.Csenet.HairColor ?? "";
            }
            else
            {
              var field1 = GetField(export.HairCol, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "hairColor");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.HairCol, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.EyeCol.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.EyeColor))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.EyeColor = export.Csenet.EyeColor ?? "";
            }
            else
            {
              var field1 = GetField(export.EyeCol, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "eyeColor");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.EyeCol, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Height.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.Csenet.HeightFt.GetValueOrDefault() > 0)
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.HeightFt =
                export.Csenet.HeightFt.GetValueOrDefault();
              local.CseCsePerson.HeightIn =
                export.Csenet.HeightIn.GetValueOrDefault();
            }
            else
            {
              var field1 = GetField(export.Height, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "heightFt");

              field2.Color = "red";
              field2.Protected = true;

              var field3 = GetField(export.Csenet, "heightIn");

              field3.Color = "red";
              field3.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Height, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Race.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.Race))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.Race = export.Csenet.Race ?? "";
            }
            else
            {
              var field1 = GetField(export.Race, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "race");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Race, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Sex.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.Sex))
            {
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePersonsWorkSet.Sex = export.Csenet.Sex ?? Spaces(1);
            }
            else
            {
              var field1 = GetField(export.Sex, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "sex");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Sex, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Pob.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.PlaceOfBirth))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CseCsePerson.BirthPlaceCity =
                export.Csenet.PlaceOfBirth ?? "";
            }
            else
            {
              var field1 = GetField(export.Pob, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "placeOfBirth");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Pob, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Dob.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.Csenet.DateOfBirth != null)
            {
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePersonsWorkSet.Dob = export.Csenet.DateOfBirth;
            }
            else
            {
              var field1 = GetField(export.Dob, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "dateOfBirth");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Dob, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Ssn.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.Ssn))
            {
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePersonsWorkSet.Ssn = export.Csenet.Ssn ?? Spaces(9);
            }
            else
            {
              var field1 = GetField(export.Ssn, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "ssn");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Ssn, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Mn.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.MiddleName))
            {
              local.UpdCsePerson.Flag = "Y";
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePerson.NameMiddle = export.Csenet.MiddleName ?? "";
              local.CseCsePersonsWorkSet.MiddleInitial =
                export.Csenet.MiddleName ?? Spaces(1);
            }
            else
            {
              var field1 = GetField(export.Mn, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "middleName");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Mn, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Fn.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.NameFirst))
            {
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePersonsWorkSet.FirstName = export.Csenet.NameFirst;
            }
            else
            {
              var field1 = GetField(export.Fn, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "nameFirst");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Fn, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Ln.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.Csenet.NameLast))
            {
              local.UpdCsePersonWorkset.Flag = "Y";
              local.CseCsePersonsWorkSet.LastName = export.Csenet.NameLast ?? Spaces
                (17);
            }
            else
            {
              var field1 = GetField(export.Ln, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Csenet, "nameLast");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Ln, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.UpdCsePerson.Flag) == 'Y' || AsChar
          (local.UpdCsePersonWorkset.Flag) == 'Y' || !
          IsEmpty(local.UpdAlias1.Flag) || !IsEmpty(local.UpdAlias2.Flag))
        {
          UseSiProcessComparisonData();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.AltSsn1.SelectChar = local.AltSsn1.SelectChar;
            export.AltSsn2.SelectChar = local.AltSsn2.SelectChar;
            export.Dob.SelectChar = local.Dob.SelectChar;
            export.EyeCol.SelectChar = local.EyeCol.SelectChar;
            export.Fn.SelectChar = local.Fn.SelectChar;
            export.HairCol.SelectChar = local.HairCol.SelectChar;
            export.Height.SelectChar = local.Height.SelectChar;
            export.Ln.SelectChar = local.Ln.SelectChar;
            export.Mn.SelectChar = local.Mn.SelectChar;
            export.OtherId.SelectChar = local.OtherId.SelectChar;
            export.Pob.SelectChar = local.Pob.SelectChar;
            export.Race.SelectChar = local.Race.SelectChar;
            export.Sex.SelectChar = local.Sex.SelectChar;
            export.Ssn.SelectChar = local.Ssn.SelectChar;
            export.Weight.SelectChar = local.Weight.SelectChar;
            export.Export1St.Assign(local.Local1St);
            export.Export2Nd.Assign(local.Local2Nd);
            export.CseCsePerson.Assign(local.CseCsePerson);
            export.CseCsePersonsWorkSet.Assign(local.CseCsePersonsWorkSet);
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }
        else
        {
          var field = GetField(export.Ln, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "SCNX":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_ILOC";
        }
        else if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 0)
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
        else if (export.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else if (export.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

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
      case "COMP":
        export.Case1.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        break;
      case "ISUP":
        ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";

        break;
      case "IMIS":
        ExitState = "ECO_LNK_TO_CSE_MISC";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

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

  private void UseSiCheckApCurrHist()
  {
    var useImport = new SiCheckApCurrHist.Import();
    var useExport = new SiCheckApCurrHist.Export();

    MoveInterstateCase1(export.InterstateCase, useImport.InterstateCase);

    Call(SiCheckApCurrHist.Execute, useImport, useExport);

    local.ApHistoryInd.Flag = useExport.ApHistoryInd.Flag;
    local.ApCurrentInd.Flag = useExport.ApCurrentInd.Flag;
  }

  private void UseSiProcessComparisonData()
  {
    var useImport = new SiProcessComparisonData.Import();
    var useExport = new SiProcessComparisonData.Export();

    useImport.UpdAlias2.Flag = local.UpdAlias2.Flag;
    useImport.UpdAlias1.Flag = local.UpdAlias1.Flag;
    useImport.UpdCsePersonInd.Flag = local.UpdCsePerson.Flag;
    useImport.UpdCsePersnWrkSetInd.Flag = local.UpdCsePersonWorkset.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CseCsePersonsWorkSet);
    useImport.CsePerson.Assign(local.CseCsePerson);
    useImport.Alias1.Assign(local.Local1St);
    useImport.Alias2.Assign(local.Local2Nd);

    Call(SiProcessComparisonData.Execute, useImport, useExport);
  }

  private void UseSiReadCsenetApIdData()
  {
    var useImport = new SiReadCsenetApIdData.Import();
    var useExport = new SiReadCsenetApIdData.Export();

    MoveInterstateCase1(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetApIdData.Execute, useImport, useExport);

    export.Csenet.Assign(useExport.InterstateApIdentification);
    export.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiRetrieveApIdCompareInfo()
  {
    var useImport = new SiRetrieveApIdCompareInfo.Import();
    var useExport = new SiRetrieveApIdCompareInfo.Export();

    useImport.Apid.Assign(export.CseCsePersonsWorkSet);
    MoveInterstateCase2(import.InterstateCase, useImport.InterstateCase);

    Call(SiRetrieveApIdCompareInfo.Execute, useImport, useExport);

    export.Export2Nd.Assign(useExport.Export2Nd);
    export.Export1St.Assign(useExport.Export1St);
    export.CseCsePerson.Assign(useExport.CsePerson);
    export.CseCsePersonsWorkSet.Assign(useExport.Cse);
    export.OwnedByAe.Flag = useExport.OwnedByAe.Flag;
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
    /// <summary>A PartListGroup group.</summary>
    [Serializable]
    public class PartListGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePersonsWorkSet g;
    }

    /// <summary>
    /// A value of Import1St.
    /// </summary>
    [JsonPropertyName("import1St")]
    public CsePersonsWorkSet Import1St
    {
      get => import1St ??= new();
      set => import1St = value;
    }

    /// <summary>
    /// A value of Import2Nd.
    /// </summary>
    [JsonPropertyName("import2Nd")]
    public CsePersonsWorkSet Import2Nd
    {
      get => import2Nd ??= new();
      set => import2Nd = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of CseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("cseCsePersonsWorkSet")]
    public CsePersonsWorkSet CseCsePersonsWorkSet
    {
      get => cseCsePersonsWorkSet ??= new();
      set => cseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CseCsePerson.
    /// </summary>
    [JsonPropertyName("cseCsePerson")]
    public CsePerson CseCsePerson
    {
      get => cseCsePerson ??= new();
      set => cseCsePerson = value;
    }

    /// <summary>
    /// A value of Csenet.
    /// </summary>
    [JsonPropertyName("csenet")]
    public InterstateApIdentification Csenet
    {
      get => csenet ??= new();
      set => csenet = value;
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
    /// A value of AltSsn2.
    /// </summary>
    [JsonPropertyName("altSsn2")]
    public Common AltSsn2
    {
      get => altSsn2 ??= new();
      set => altSsn2 = value;
    }

    /// <summary>
    /// A value of AltSsn1.
    /// </summary>
    [JsonPropertyName("altSsn1")]
    public Common AltSsn1
    {
      get => altSsn1 ??= new();
      set => altSsn1 = value;
    }

    /// <summary>
    /// A value of OtherId.
    /// </summary>
    [JsonPropertyName("otherId")]
    public Common OtherId
    {
      get => otherId ??= new();
      set => otherId = value;
    }

    /// <summary>
    /// A value of Weight.
    /// </summary>
    [JsonPropertyName("weight")]
    public Common Weight
    {
      get => weight ??= new();
      set => weight = value;
    }

    /// <summary>
    /// A value of HairCol.
    /// </summary>
    [JsonPropertyName("hairCol")]
    public Common HairCol
    {
      get => hairCol ??= new();
      set => hairCol = value;
    }

    /// <summary>
    /// A value of EyeCol.
    /// </summary>
    [JsonPropertyName("eyeCol")]
    public Common EyeCol
    {
      get => eyeCol ??= new();
      set => eyeCol = value;
    }

    /// <summary>
    /// A value of Height.
    /// </summary>
    [JsonPropertyName("height")]
    public Common Height
    {
      get => height ??= new();
      set => height = value;
    }

    /// <summary>
    /// A value of Race.
    /// </summary>
    [JsonPropertyName("race")]
    public Common Race
    {
      get => race ??= new();
      set => race = value;
    }

    /// <summary>
    /// A value of Sex.
    /// </summary>
    [JsonPropertyName("sex")]
    public Common Sex
    {
      get => sex ??= new();
      set => sex = value;
    }

    /// <summary>
    /// A value of Pob.
    /// </summary>
    [JsonPropertyName("pob")]
    public Common Pob
    {
      get => pob ??= new();
      set => pob = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public Common Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public Common Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    /// <summary>
    /// A value of Mn.
    /// </summary>
    [JsonPropertyName("mn")]
    public Common Mn
    {
      get => mn ??= new();
      set => mn = value;
    }

    /// <summary>
    /// A value of Fn.
    /// </summary>
    [JsonPropertyName("fn")]
    public Common Fn
    {
      get => fn ??= new();
      set => fn = value;
    }

    /// <summary>
    /// A value of Ln.
    /// </summary>
    [JsonPropertyName("ln")]
    public Common Ln
    {
      get => ln ??= new();
      set => ln = value;
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
    /// Gets a value of PartList.
    /// </summary>
    [JsonIgnore]
    public Array<PartListGroup> PartList => partList ??= new(
      PartListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PartList for json serialization.
    /// </summary>
    [JsonPropertyName("partList")]
    [Computed]
    public IList<PartListGroup> PartList_Json
    {
      get => partList;
      set => PartList.Assign(value);
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
    /// A value of OwnedByAe.
    /// </summary>
    [JsonPropertyName("ownedByAe")]
    public Common OwnedByAe
    {
      get => ownedByAe ??= new();
      set => ownedByAe = value;
    }

    private CsePersonsWorkSet import1St;
    private CsePersonsWorkSet import2Nd;
    private CsePersonLicense csePersonLicense;
    private CsePersonsWorkSet cseCsePersonsWorkSet;
    private CsePerson cseCsePerson;
    private InterstateApIdentification csenet;
    private InterstateCase interstateCase;
    private Common altSsn2;
    private Common altSsn1;
    private Common otherId;
    private Common weight;
    private Common hairCol;
    private Common eyeCol;
    private Common height;
    private Common race;
    private Common sex;
    private Common pob;
    private Common dob;
    private Common ssn;
    private Common mn;
    private Common fn;
    private Common ln;
    private Standard standard;
    private Array<PartListGroup> partList;
    private NextTranInfo hidden;
    private Common ownedByAe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PartListGroup group.</summary>
    [Serializable]
    public class PartListGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePersonsWorkSet g;
    }

    /// <summary>
    /// A value of Export1St.
    /// </summary>
    [JsonPropertyName("export1St")]
    public CsePersonsWorkSet Export1St
    {
      get => export1St ??= new();
      set => export1St = value;
    }

    /// <summary>
    /// A value of Export2Nd.
    /// </summary>
    [JsonPropertyName("export2Nd")]
    public CsePersonsWorkSet Export2Nd
    {
      get => export2Nd ??= new();
      set => export2Nd = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of CseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("cseCsePersonsWorkSet")]
    public CsePersonsWorkSet CseCsePersonsWorkSet
    {
      get => cseCsePersonsWorkSet ??= new();
      set => cseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CseCsePerson.
    /// </summary>
    [JsonPropertyName("cseCsePerson")]
    public CsePerson CseCsePerson
    {
      get => cseCsePerson ??= new();
      set => cseCsePerson = value;
    }

    /// <summary>
    /// A value of Csenet.
    /// </summary>
    [JsonPropertyName("csenet")]
    public InterstateApIdentification Csenet
    {
      get => csenet ??= new();
      set => csenet = value;
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
    /// A value of AltSsn2.
    /// </summary>
    [JsonPropertyName("altSsn2")]
    public Common AltSsn2
    {
      get => altSsn2 ??= new();
      set => altSsn2 = value;
    }

    /// <summary>
    /// A value of AltSsn1.
    /// </summary>
    [JsonPropertyName("altSsn1")]
    public Common AltSsn1
    {
      get => altSsn1 ??= new();
      set => altSsn1 = value;
    }

    /// <summary>
    /// A value of OtherId.
    /// </summary>
    [JsonPropertyName("otherId")]
    public Common OtherId
    {
      get => otherId ??= new();
      set => otherId = value;
    }

    /// <summary>
    /// A value of Weight.
    /// </summary>
    [JsonPropertyName("weight")]
    public Common Weight
    {
      get => weight ??= new();
      set => weight = value;
    }

    /// <summary>
    /// A value of HairCol.
    /// </summary>
    [JsonPropertyName("hairCol")]
    public Common HairCol
    {
      get => hairCol ??= new();
      set => hairCol = value;
    }

    /// <summary>
    /// A value of EyeCol.
    /// </summary>
    [JsonPropertyName("eyeCol")]
    public Common EyeCol
    {
      get => eyeCol ??= new();
      set => eyeCol = value;
    }

    /// <summary>
    /// A value of Height.
    /// </summary>
    [JsonPropertyName("height")]
    public Common Height
    {
      get => height ??= new();
      set => height = value;
    }

    /// <summary>
    /// A value of Race.
    /// </summary>
    [JsonPropertyName("race")]
    public Common Race
    {
      get => race ??= new();
      set => race = value;
    }

    /// <summary>
    /// A value of Sex.
    /// </summary>
    [JsonPropertyName("sex")]
    public Common Sex
    {
      get => sex ??= new();
      set => sex = value;
    }

    /// <summary>
    /// A value of Pob.
    /// </summary>
    [JsonPropertyName("pob")]
    public Common Pob
    {
      get => pob ??= new();
      set => pob = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public Common Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public Common Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    /// <summary>
    /// A value of Mn.
    /// </summary>
    [JsonPropertyName("mn")]
    public Common Mn
    {
      get => mn ??= new();
      set => mn = value;
    }

    /// <summary>
    /// A value of Fn.
    /// </summary>
    [JsonPropertyName("fn")]
    public Common Fn
    {
      get => fn ??= new();
      set => fn = value;
    }

    /// <summary>
    /// A value of Ln.
    /// </summary>
    [JsonPropertyName("ln")]
    public Common Ln
    {
      get => ln ??= new();
      set => ln = value;
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
    /// Gets a value of PartList.
    /// </summary>
    [JsonIgnore]
    public Array<PartListGroup> PartList => partList ??= new(
      PartListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PartList for json serialization.
    /// </summary>
    [JsonPropertyName("partList")]
    [Computed]
    public IList<PartListGroup> PartList_Json
    {
      get => partList;
      set => PartList.Assign(value);
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
    /// A value of OwnedByAe.
    /// </summary>
    [JsonPropertyName("ownedByAe")]
    public Common OwnedByAe
    {
      get => ownedByAe ??= new();
      set => ownedByAe = value;
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

    private CsePersonsWorkSet export1St;
    private CsePersonsWorkSet export2Nd;
    private CsePersonLicense csePersonLicense;
    private CsePersonsWorkSet cseCsePersonsWorkSet;
    private CsePerson cseCsePerson;
    private InterstateApIdentification csenet;
    private InterstateCase interstateCase;
    private Common altSsn2;
    private Common altSsn1;
    private Common otherId;
    private Common weight;
    private Common hairCol;
    private Common eyeCol;
    private Common height;
    private Common race;
    private Common sex;
    private Common pob;
    private Common dob;
    private Common ssn;
    private Common mn;
    private Common fn;
    private Common ln;
    private Standard standard;
    private Array<PartListGroup> partList;
    private NextTranInfo hidden;
    private Common ownedByAe;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZdelLocalOwnedByAe.
    /// </summary>
    [JsonPropertyName("zdelLocalOwnedByAe")]
    public Common ZdelLocalOwnedByAe
    {
      get => zdelLocalOwnedByAe ??= new();
      set => zdelLocalOwnedByAe = value;
    }

    /// <summary>
    /// A value of CseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("cseCsePersonsWorkSet")]
    public CsePersonsWorkSet CseCsePersonsWorkSet
    {
      get => cseCsePersonsWorkSet ??= new();
      set => cseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CseCsePerson.
    /// </summary>
    [JsonPropertyName("cseCsePerson")]
    public CsePerson CseCsePerson
    {
      get => cseCsePerson ??= new();
      set => cseCsePerson = value;
    }

    /// <summary>
    /// A value of Local1St.
    /// </summary>
    [JsonPropertyName("local1St")]
    public CsePersonsWorkSet Local1St
    {
      get => local1St ??= new();
      set => local1St = value;
    }

    /// <summary>
    /// A value of Local2Nd.
    /// </summary>
    [JsonPropertyName("local2Nd")]
    public CsePersonsWorkSet Local2Nd
    {
      get => local2Nd ??= new();
      set => local2Nd = value;
    }

    /// <summary>
    /// A value of AltSsn2.
    /// </summary>
    [JsonPropertyName("altSsn2")]
    public Common AltSsn2
    {
      get => altSsn2 ??= new();
      set => altSsn2 = value;
    }

    /// <summary>
    /// A value of AltSsn1.
    /// </summary>
    [JsonPropertyName("altSsn1")]
    public Common AltSsn1
    {
      get => altSsn1 ??= new();
      set => altSsn1 = value;
    }

    /// <summary>
    /// A value of OtherId.
    /// </summary>
    [JsonPropertyName("otherId")]
    public Common OtherId
    {
      get => otherId ??= new();
      set => otherId = value;
    }

    /// <summary>
    /// A value of Weight.
    /// </summary>
    [JsonPropertyName("weight")]
    public Common Weight
    {
      get => weight ??= new();
      set => weight = value;
    }

    /// <summary>
    /// A value of HairCol.
    /// </summary>
    [JsonPropertyName("hairCol")]
    public Common HairCol
    {
      get => hairCol ??= new();
      set => hairCol = value;
    }

    /// <summary>
    /// A value of EyeCol.
    /// </summary>
    [JsonPropertyName("eyeCol")]
    public Common EyeCol
    {
      get => eyeCol ??= new();
      set => eyeCol = value;
    }

    /// <summary>
    /// A value of Height.
    /// </summary>
    [JsonPropertyName("height")]
    public Common Height
    {
      get => height ??= new();
      set => height = value;
    }

    /// <summary>
    /// A value of Race.
    /// </summary>
    [JsonPropertyName("race")]
    public Common Race
    {
      get => race ??= new();
      set => race = value;
    }

    /// <summary>
    /// A value of Sex.
    /// </summary>
    [JsonPropertyName("sex")]
    public Common Sex
    {
      get => sex ??= new();
      set => sex = value;
    }

    /// <summary>
    /// A value of Pob.
    /// </summary>
    [JsonPropertyName("pob")]
    public Common Pob
    {
      get => pob ??= new();
      set => pob = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public Common Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public Common Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    /// <summary>
    /// A value of Mn.
    /// </summary>
    [JsonPropertyName("mn")]
    public Common Mn
    {
      get => mn ??= new();
      set => mn = value;
    }

    /// <summary>
    /// A value of Fn.
    /// </summary>
    [JsonPropertyName("fn")]
    public Common Fn
    {
      get => fn ??= new();
      set => fn = value;
    }

    /// <summary>
    /// A value of Ln.
    /// </summary>
    [JsonPropertyName("ln")]
    public Common Ln
    {
      get => ln ??= new();
      set => ln = value;
    }

    /// <summary>
    /// A value of UpdAlias2.
    /// </summary>
    [JsonPropertyName("updAlias2")]
    public Common UpdAlias2
    {
      get => updAlias2 ??= new();
      set => updAlias2 = value;
    }

    /// <summary>
    /// A value of UpdAlias1.
    /// </summary>
    [JsonPropertyName("updAlias1")]
    public Common UpdAlias1
    {
      get => updAlias1 ??= new();
      set => updAlias1 = value;
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
    /// A value of UpdCsePersonWorkset.
    /// </summary>
    [JsonPropertyName("updCsePersonWorkset")]
    public Common UpdCsePersonWorkset
    {
      get => updCsePersonWorkset ??= new();
      set => updCsePersonWorkset = value;
    }

    /// <summary>
    /// A value of UpdCsePerson.
    /// </summary>
    [JsonPropertyName("updCsePerson")]
    public Common UpdCsePerson
    {
      get => updCsePerson ??= new();
      set => updCsePerson = value;
    }

    private Common zdelLocalOwnedByAe;
    private CsePersonsWorkSet cseCsePersonsWorkSet;
    private CsePerson cseCsePerson;
    private CsePersonsWorkSet local1St;
    private CsePersonsWorkSet local2Nd;
    private Common altSsn2;
    private Common altSsn1;
    private Common otherId;
    private Common weight;
    private Common hairCol;
    private Common eyeCol;
    private Common height;
    private Common race;
    private Common sex;
    private Common pob;
    private Common dob;
    private Common ssn;
    private Common mn;
    private Common fn;
    private Common ln;
    private Common updAlias2;
    private Common updAlias1;
    private Common apHistoryInd;
    private Common apCurrentInd;
    private Common updCsePersonWorkset;
    private Common updCsePerson;
  }
#endregion
}
