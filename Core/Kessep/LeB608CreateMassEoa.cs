// Program: LE_B608_CREATE_MASS_EOA, ID: 945200014, model: 746.
// Short name: SWEB608P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B608_CREATE_MASS_EOA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB608CreateMassEoa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B608_CREATE_MASS_EOA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB608CreateMassEoa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB608CreateMassEoa.
  /// </summary>
  public LeB608CreateMassEoa(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 08/26/13     Anita Hockman   cq 41132    Need to mass produce the Entry 
    // of Appearance
    //                                          
    // doc for the privatization of CSS.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    //  This program creates the EOA legal documents using an extract file from 
    // a previous job
    //                    SWELB407 which has been sorted by court order.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ReadEabFileHandling.Action = "READ";
    local.Open.Action = "OPEN";
    local.Close.Action = "CLOSE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Rpt.ReportNumber = 1;
    UseSpEabWriteDocument1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseLeB608ReadExtractFile2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!Equal(local.EabFileHandling.Status, "00"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    do
    {
      for(local.Ap.Index = 0; local.Ap.Index < local.Ap.Count; ++local.Ap.Index)
      {
        if (!local.Ap.CheckSize())
        {
          break;
        }

        local.Ap.Update.Ap1.FormattedName = "";
      }

      local.Ap.CheckIndex();
      local.Ap.Count = 0;
      local.Ap.Index = -1;

      for(local.Ar.Index = 0; local.Ar.Index < local.Ar.Count; ++local.Ar.Index)
      {
        if (!local.Ar.CheckSize())
        {
          break;
        }

        local.Ar.Update.Ar1.FormattedName = "";
      }

      local.Ar.CheckIndex();
      local.Ar.Count = 0;
      local.Ar.Index = -1;

      for(local.Attrony.Index = 0; local.Attrony.Index < local.Attrony.Count; ++
        local.Attrony.Index)
      {
        if (!local.Attrony.CheckSize())
        {
          break;
        }

        local.Attrony.Update.Attr.FormattedName = "";
        local.Attrony.Update.PrivateAttorneyAddress.Street1 = "";
        local.Attrony.Update.PrivateAttorneyAddress.Street2 = "";
        local.Attrony.Update.PrivateAttorneyAddress.City = "";
        local.Attrony.Update.PrivateAttorneyAddress.State = "";
        local.Attrony.Update.PrivateAttorneyAddress.ZipCode5 = "";
      }

      local.Attrony.CheckIndex();
      local.Attrony.Count = 0;
      local.Attrony.Index = -1;

      for(local.LegalCaptionGroupView.Index = 0; local
        .LegalCaptionGroupView.Index < local.LegalCaptionGroupView.Count; ++
        local.LegalCaptionGroupView.Index)
      {
        if (!local.LegalCaptionGroupView.CheckSize())
        {
          break;
        }

        local.LegalCaptionGroupView.Update.LegalCaption.Line =
          Spaces(CourtCaption.Line_MaxLength);
        local.LegalCaptionGroupView.Update.LegalCaption.Number = 0;
      }

      local.LegalCaptionGroupView.CheckIndex();
      local.LegalCaptionGroupView.Count = 0;
      local.LegalCaptionGroupView.Index = -1;
      local.PersonPrivateAttorney.FirstName = "";
      local.PersonPrivateAttorney.LastName = "";
      local.AttryCertifiactionNum.Text5 = "";
      local.Loacl.Street1 = "";
      local.Loacl.Street2 = "";
      local.Loacl.City = "";
      local.Loacl.State = "";
      local.Loacl.ZipCode5 = "";
      local.LegalAction.Identifier = 0;
      local.LegalAction.StandardNumber = "";
      local.Fips.CountyDescription = "";
      local.Tribunal.DocumentHeader1 =
        Spaces(Tribunal.DocumentHeader1_MaxLength);
      local.Tribunal.DocumentHeader2 =
        Spaces(Tribunal.DocumentHeader2_MaxLength);
      local.Tribunal.DocumentHeader3 =
        Spaces(Tribunal.DocumentHeader3_MaxLength);
      local.Tribunal.DocumentHeader4 =
        Spaces(Tribunal.DocumentHeader4_MaxLength);
      local.Tribunal.DocumentHeader5 =
        Spaces(Tribunal.DocumentHeader5_MaxLength);
      local.Tribunal.DocumentHeader6 =
        Spaces(Tribunal.DocumentHeader6_MaxLength);
      local.Name.FirstName = "";
      local.Name.LastName = "";
      local.Name.FormattedName = "";
      local.Name.Number = "";
      local.ReadCsePersonsWorkSet.Number = "";
      local.ReadPersonPrivateAttorney.Identifier = 0;

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Get a record from the sorted/summed extract file.
      // --
      // --  Note that the external can return more data than what we actually 
      // need.  Not all views need to be returned for what we're doing here.
      // -------------------------------------------------------------------------------------------------------------------------
      UseLeB608ReadExtractFile1();

      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        break;
      }

      if (!Equal(local.EabFileHandling.Status, "00") && !
        Equal(local.EabFileHandling.Status, "EF"))
      {
        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

      local.AttryTitle.Text17 = "Contract Attorney";

      switch(TrimEnd(local.LeadAtty.Text2))
      {
        case "1":
          local.PersonPrivateAttorney.FirstName = "Amy";
          local.PersonPrivateAttorney.LastName = "Raymond";
          local.AttryCertifiactionNum.Text5 = "20839";
          local.OfficeName.Text37 = "YoungWilliams Child Support Services";

          switch(TrimEnd(local.Fips.CountyAbbreviation ?? ""))
          {
            case "PT":
              local.Loacl.Street1 = "Townsite Plaza IV";
              local.Loacl.Street2 = "120 SE 6th St., Suite 106";
              local.Loacl.City = "Topeka";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66603";
              local.AttyFaxPhone.Text12 = "785-233-0822";
              local.AttyFaxPhone.Text17 = "Fax: 785-233-1560";
              local.Att.EmailAddress = "Topeka@ywcss.com";

              break;
            case "WB":
              local.Loacl.Street1 = "Townsite Plaza IV";
              local.Loacl.Street2 = "120 SE 6th St., Suite 106";
              local.Loacl.City = "Topeka";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66603";
              local.AttyFaxPhone.Text12 = "785-233-0822";
              local.AttyFaxPhone.Text17 = "Fax: 785-233-1560";
              local.Att.EmailAddress = "Topeka@ywcss.com";

              break;
            case "CS":
              local.Loacl.Street1 = "Townsite Plaza IV";
              local.Loacl.Street2 = "120 SE 6th St., Suite 106";
              local.Loacl.City = "Topeka";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66603";
              local.AttyFaxPhone.Text12 = "785-233-0822";
              local.AttyFaxPhone.Text17 = "Fax: 785-233-1560";
              local.Att.EmailAddress = "Topeka@ywcss.com";

              break;
            case "LY":
              local.Loacl.Street1 = "Townsite Plaza IV";
              local.Loacl.Street2 = "120 SE 6th St., Suite 106";
              local.Loacl.City = "Topeka";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66603";
              local.AttyFaxPhone.Text12 = "785-233-0822";
              local.AttyFaxPhone.Text17 = "Fax: 785-233-1560";
              local.Att.EmailAddress = "Topeka@ywcss.com";

              break;
            default:
              continue;

              // only processing certian records
              break;
          }

          break;
        case "2":
          local.PersonPrivateAttorney.FirstName = "Audrey";
          local.PersonPrivateAttorney.LastName = "Magana";
          local.AttryCertifiactionNum.Text5 = "11751";
          local.OfficeName.Text37 = "YoungWilliams Child Support Services";

          switch(TrimEnd(local.Fips.CountyAbbreviation ?? ""))
          {
            case "MN":
              local.Loacl.Street1 = "711 Southwind Drive";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Junction City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66441";
              local.AttyFaxPhone.Text12 = "785-530-5040";
              local.AttyFaxPhone.Text17 = "Fax: 785-530-5041";
              local.Att.EmailAddress = "JunctionCity@ywcss.com";

              break;
            case "MR":
              local.Loacl.Street1 = "711 Southwind Drive";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Junction City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66441";
              local.AttyFaxPhone.Text12 = "785-530-5040";
              local.AttyFaxPhone.Text17 = "Fax: 785-530-5041";
              local.Att.EmailAddress = "JunctionCity@ywcss.com";

              break;
            case "GE":
              local.Loacl.Street1 = "711 Southwind Drive";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Junction City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66441";
              local.AttyFaxPhone.Text12 = "785-530-5040";
              local.AttyFaxPhone.Text17 = "Fax: 785-530-5041";
              local.Att.EmailAddress = "JunctionCity@ywcss.com";

              break;
            case "DK":
              local.Loacl.Street1 = "110 W. Walnut Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Salina";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67401";
              local.AttyFaxPhone.Text12 = "785-823-6600";
              local.AttyFaxPhone.Text17 = "Fax: 785-823-1606";
              local.Att.EmailAddress = "Salina@ywcss.com";

              break;
            case "HV":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "MP":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "CY":
              local.Loacl.Street1 = "711 Southwind Drive";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Junction City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66441";
              local.AttyFaxPhone.Text12 = "785-530-5040";
              local.AttyFaxPhone.Text17 = "Fax: 785-530-5041";
              local.Att.EmailAddress = "JunctionCity@ywcss.com";

              break;
            case "RL":
              local.Loacl.Street1 = "711 Southwind Drive";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Junction City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66441";
              local.AttyFaxPhone.Text12 = "785-530-5040";
              local.AttyFaxPhone.Text17 = "Fax: 785-530-5041";
              local.Att.EmailAddress = "JunctionCity@ywcss.com";

              break;
            case "RN":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "OT":
              local.Loacl.Street1 = "110 W. Walnut Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Salina";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67401";
              local.AttyFaxPhone.Text12 = "785-823-6600";
              local.AttyFaxPhone.Text17 = "Fax: 785-823-1606";
              local.Att.EmailAddress = "Salina@ywcss.com";

              break;
            case "SA":
              local.Loacl.Street1 = "110 W. Walnut Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Salina";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67401";
              local.AttyFaxPhone.Text12 = "785-823-6600";
              local.AttyFaxPhone.Text17 = "Fax: 785-823-1606";
              local.Att.EmailAddress = "Salina@ywcss.com";

              break;
            case "BA":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "HP":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "KM":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "PR":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            case "SU":
              local.Loacl.Street1 = "335 N. Washington Street";
              local.Loacl.Street2 = "Ste 200";
              local.Loacl.City = "Hutchinson";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67501";
              local.AttyFaxPhone.Text12 = "620-860-1502";
              local.AttyFaxPhone.Text17 = "Fax: 620-860-1689";
              local.Att.EmailAddress = "Hutchinson@ywcss.com";

              break;
            default:
              continue;

              // only processing certian records
              break;
          }

          break;
        case "3":
          local.PersonPrivateAttorney.FirstName = "Marcilyn";
          local.PersonPrivateAttorney.LastName = "Martinez";
          local.AttryCertifiactionNum.Text5 = "18463";
          local.Loacl.Street1 = "500 N Rogers";
          local.Loacl.Street2 = "";
          local.Loacl.City = "Olathe";
          local.Loacl.State = "KS";
          local.Loacl.ZipCode5 = "66062";

          break;
        case "4":
          local.PersonPrivateAttorney.FirstName = "Brandi";
          local.PersonPrivateAttorney.LastName = "Ridgeway";
          local.AttryCertifiactionNum.Text5 = "15632";
          local.OfficeName.Text37 = "YoungWilliams Child Support Services";

          switch(TrimEnd(local.Fips.CountyAbbreviation ?? ""))
          {
            case "BB":
              local.Loacl.Street1 = "215 E. Maple Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Columbus";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66725";
              local.AttyFaxPhone.Text12 = "620-429-1346";
              local.AttyFaxPhone.Text17 = "Fax: 620-429-1353";
              local.Att.EmailAddress = "Columbus@ywcss.com";

              break;
            case "LN":
              local.Loacl.Street1 = "215 E. Maple Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Columbus";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66725";
              local.AttyFaxPhone.Text12 = "620-429-1346";
              local.AttyFaxPhone.Text17 = "Fax: 620-429-1353";
              local.Att.EmailAddress = "Columbus@ywcss.com";

              break;
            case "CK":
              local.Loacl.Street1 = "215 E. Maple Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Columbus";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66725";
              local.AttyFaxPhone.Text12 = "620-429-1346";
              local.AttyFaxPhone.Text17 = "Fax: 620-429-1353";
              local.Att.EmailAddress = "Columbus@ywcss.com";

              break;
            case "CR":
              local.Loacl.Street1 = "215 E. Maple Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Columbus";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66725";
              local.AttyFaxPhone.Text12 = "620-429-1346";
              local.AttyFaxPhone.Text17 = "Fax: 620-429-1353";
              local.Att.EmailAddress = "Columbus@ywcss.com";

              break;
            case "LB":
              local.Loacl.Street1 = "215 E. Maple Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Columbus";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "66725";
              local.AttyFaxPhone.Text12 = "620-429-1346";
              local.AttyFaxPhone.Text17 = "Fax: 620-429-1353";
              local.Att.EmailAddress = "Columbus@ywcss.com";

              break;
            case "BU":
              local.Loacl.Street1 = "210 S. Main Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "El Dorado";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67042";
              local.AttyFaxPhone.Text12 = "316-321-0052";
              local.AttyFaxPhone.Text17 = "Fax: 316-321-0652";
              local.Att.EmailAddress = "Eldorado@ywcss.com";

              break;
            case "EK":
              local.Loacl.Street1 = "210 S. Main Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "El Dorado";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67042";
              local.AttyFaxPhone.Text12 = "316-321-0052";
              local.AttyFaxPhone.Text17 = "Fax: 316-321-0652";
              local.Att.EmailAddress = "Eldorado@ywcss.com";

              break;
            case "GW":
              local.Loacl.Street1 = "210 S. Main Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "El Dorado";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67042";
              local.AttyFaxPhone.Text12 = "316-321-0052";
              local.AttyFaxPhone.Text17 = "Fax: 316-321-0652";
              local.Att.EmailAddress = "Eldorado@ywcss.com";

              break;
            case "CQ":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            case "MG":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            case "CL":
              local.Loacl.Street1 = "210 S. Main Street";
              local.Loacl.Street2 = "";
              local.Loacl.City = "El Dorado";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67042";
              local.AttyFaxPhone.Text12 = "316-321-0052";
              local.AttyFaxPhone.Text17 = "Fax: 316-321-0652";
              local.Att.EmailAddress = "Eldorado@ywcss.com";

              break;
            case "AL":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            case "NO":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            case "WL":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            case "WO":
              local.Loacl.Street1 = "200 Arco Place, Suite 426";
              local.Loacl.Street2 = "";
              local.Loacl.City = "Independence";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67301";
              local.AttyFaxPhone.Text12 = "620-331-9999";
              local.AttyFaxPhone.Text17 = "Fax: 620-331-9998";
              local.Att.EmailAddress = "Independence@ywcss.com";

              break;
            default:
              continue;

              // only processing certian records
              break;
          }

          break;
        case "5":
          local.PersonPrivateAttorney.FirstName = "Eric";
          local.PersonPrivateAttorney.LastName = "Lawrence";
          local.AttryCertifiactionNum.Text5 = "17765";
          local.Loacl.Street1 = "Security Bank Bdlg";
          local.Loacl.Street2 = "707 Minnesota Ave Ste 500";
          local.Loacl.City = "Kansas City";
          local.Loacl.State = "KS";
          local.Loacl.ZipCode5 = "66101";
          local.AttyFaxPhone.Text12 = "913-333-3251";
          local.AttyFaxPhone.Text17 = "Fax: 913-333-3251";
          local.Att.EmailAddress = "Eric.Lawrence@dcf.ks.gov";

          break;
        case "6":
          local.PersonPrivateAttorney.FirstName = "Lee";
          local.PersonPrivateAttorney.LastName = "Fisher";
          local.AttryCertifiactionNum.Text5 = "15565";
          local.Loacl.Street1 = "2712 Plaza Ave";
          local.Loacl.Street2 = "";
          local.Loacl.City = "Hays";
          local.Loacl.State = "KS";
          local.Loacl.ZipCode5 = "67601";
          local.AttyFaxPhone.Text12 = "785-623-4515";
          local.AttyFaxPhone.Text17 = "Fax: 785-621-2551";
          local.Att.EmailAddress = "Lee.Fisher@dcf.ks.gov";

          break;
        case "7":
          local.PersonPrivateAttorney.FirstName = "Daniel";
          local.PersonPrivateAttorney.LastName = "Macias";
          local.AttryCertifiactionNum.Text5 = "21226";
          local.Loacl.Street1 = "300 N Main, Suite 100";
          local.Loacl.Street2 = "";
          local.Loacl.City = "Wichita";
          local.Loacl.State = "KS";
          local.Loacl.ZipCode5 = "67202";
          local.AttyFaxPhone.Text12 = "316-660-9433";
          local.AttyFaxPhone.Text17 = "Fax: 316-660-9433";
          local.Att.EmailAddress = "Daniel.Macias@dcf.ks.gov";

          break;
        case "8":
          local.OfficeName.Text37 = "YoungWilliams Child Support Services";

          switch(TrimEnd(local.Fips.CountyAbbreviation ?? ""))
          {
            case "BT":
              local.PersonPrivateAttorney.FirstName = "Gail";
              local.PersonPrivateAttorney.LastName = "Carpenter";
              local.AttryCertifiactionNum.Text5 = "12916";
              local.Loacl.Street1 = "PO Box 1994";
              local.Loacl.Street2 = "1305 Patton Road";
              local.Loacl.City = "Great Bend";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67530";
              local.AttyFaxPhone.Text12 = "620-603-6315";
              local.AttyFaxPhone.Text17 = "Fax: 620-603-6320";
              local.Att.EmailAddress = "Gail.Carpenter@dcf.ks.gov";

              break;
            case "EW":
              local.PersonPrivateAttorney.FirstName = "Gail";
              local.PersonPrivateAttorney.LastName = "Carpenter";
              local.AttryCertifiactionNum.Text5 = "12916";
              local.Loacl.Street1 = "PO Box 1994";
              local.Loacl.Street2 = "1305 Patton Road";
              local.Loacl.City = "Great Bend";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67530";
              local.AttyFaxPhone.Text12 = "620-603-6315";
              local.AttyFaxPhone.Text17 = "Fax: 620-603-6320";
              local.Att.EmailAddress = "Gail.Carpenter@dcf.ks.gov";

              break;
            case "RC":
              local.PersonPrivateAttorney.FirstName = "Gail";
              local.PersonPrivateAttorney.LastName = "Carpenter";
              local.AttryCertifiactionNum.Text5 = "12916";
              local.Loacl.Street1 = "PO Box 1994";
              local.Loacl.Street2 = "1305 Patton Road";
              local.Loacl.City = "Great Bend";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67530";
              local.AttyFaxPhone.Text12 = "620-603-6315";
              local.AttyFaxPhone.Text17 = "Fax: 620-603-6320";
              local.Att.EmailAddress = "Gail.Carpenter@dcf.ks.gov";

              break;
            case "RS":
              local.PersonPrivateAttorney.FirstName = "Gail";
              local.PersonPrivateAttorney.LastName = "Carpenter";
              local.AttryCertifiactionNum.Text5 = "12916";
              local.Loacl.Street1 = "PO Box 1994";
              local.Loacl.Street2 = "1305 Patton Road";
              local.Loacl.City = "Great Bend";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67530";
              local.AttyFaxPhone.Text12 = "620-603-6315";
              local.AttyFaxPhone.Text17 = "Fax: 620-603-6320";
              local.Att.EmailAddress = "Gail.Carpenter@dcf.ks.gov";

              break;
            case "SF":
              local.PersonPrivateAttorney.FirstName = "Gail";
              local.PersonPrivateAttorney.LastName = "Carpenter";
              local.AttryCertifiactionNum.Text5 = "12916";
              local.Loacl.Street1 = "PO Box 1994";
              local.Loacl.Street2 = "1305 Patton Road";
              local.Loacl.City = "Great Bend";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67530";
              local.AttyFaxPhone.Text12 = "620-603-6315";
              local.AttyFaxPhone.Text17 = "Fax: 620-603-6320";
              local.Att.EmailAddress = "Gail.Carpenter@dcf.ks.gov";

              break;
            case "FI":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            case "GL":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            case "HM":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            case "KE":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            case "SC":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            case "WH":
              local.PersonPrivateAttorney.FirstName = "Joseph";
              local.PersonPrivateAttorney.LastName = "Herold";
              local.AttryCertifiactionNum.Text5 = "12015";
              local.Loacl.Street1 = "PO Box 638";
              local.Loacl.Street2 = "1710 Palace Drive";
              local.Loacl.City = "Garden City";
              local.Loacl.State = "KS";
              local.Loacl.ZipCode5 = "67846";
              local.AttyFaxPhone.Text12 = "620-805-6550";
              local.AttyFaxPhone.Text17 = "Fax: 620-805-6555";
              local.Att.EmailAddress = "Joseph.Herold@dcf.ks.gov";

              break;
            default:
              continue;

              // only processing certian records
              break;
          }

          break;
        default:
          local.PersonPrivateAttorney.FirstName = "";
          local.PersonPrivateAttorney.LastName = "";
          local.AttryCertifiactionNum.Text5 = "";
          local.Loacl.Street1 = "";
          local.Loacl.Street2 = "";
          local.Loacl.City = "";
          local.Loacl.State = "";
          local.Loacl.ZipCode5 = "";
          local.AttyFaxPhone.Text12 = "";
          local.AttyFaxPhone.Text17 = "";
          local.Att.EmailAddress = "";
          local.OfficeName.Text37 = "";

          break;
      }

      do
      {
        local.ReadCsePersonsWorkSet.Number =
          Substring(local.ApNumber.Text200, 1, 10);

        if (!IsEmpty(local.ReadCsePersonsWorkSet.Number))
        {
        }
        else
        {
          break;
        }

        local.Name.Assign(local.Clear);
        UseCabReadAdabasPersonBatch();
        UseSiFormatCsePersonName();

        ++local.Ap.Index;
        local.Ap.CheckSize();

        local.Ap.Update.Ap1.FormattedName = local.Name.FormattedName;
        local.ApNumber.Text200 = Substring(local.ApNumber.Text200, 11, 190);
      }
      while(!Equal(local.ReadEabFileHandling.Status, "OK"));

      do
      {
        local.ReadCsePersonsWorkSet.Number =
          Substring(local.ArNumber.Text200, 1, 10);

        if (!IsEmpty(local.ReadCsePersonsWorkSet.Number))
        {
        }
        else
        {
          break;
        }

        local.Name.Assign(local.Clear);
        UseCabReadAdabasPersonBatch();
        UseSiFormatCsePersonName();

        ++local.Ar.Index;
        local.Ar.CheckSize();

        local.Ar.Update.Ar1.FormattedName = local.Name.FormattedName;
        local.ArNumber.Text200 = Substring(local.ArNumber.Text200, 11, 190);
      }
      while(!Equal(local.ReadEabFileHandling.Status, "OK"));

      do
      {
        local.ReadCsePersonsWorkSet.Number =
          Substring(local.AttorInfo.Text200, 1, 10);
        local.ReadPersonPrivateAttorney.Identifier =
          (int)StringToNumber(Substring(
            local.AttorInfo.Text200, WorkArea.Text200_MaxLength, 11, 2));

        if (!IsEmpty(local.ReadCsePersonsWorkSet.Number))
        {
        }
        else
        {
          break;
        }

        local.Name.Assign(local.Clear);
        local.Name.FormattedName = "";

        if (ReadPersonPrivateAttorney())
        {
          local.Name.FirstName = entities.PersonPrivateAttorney.FirstName ?? Spaces
            (12);
          local.Name.LastName = entities.PersonPrivateAttorney.LastName ?? Spaces
            (17);
          ReadPrivateAttorneyAddress();
        }
        else
        {
          continue;
        }

        UseSiFormatCsePersonName();

        ++local.Attrony.Index;
        local.Attrony.CheckSize();

        if (!IsEmpty(local.Name.FormattedName))
        {
          local.Attrony.Update.Attr.FormattedName = local.Name.FormattedName;
        }
        else
        {
          local.Attrony.Update.Attr.FormattedName =
            entities.PersonPrivateAttorney.FirmName ?? Spaces(33);
        }

        local.AttorInfo.Text200 = Substring(local.AttorInfo.Text200, 13, 188);
        local.Attrony.Update.PrivateAttorneyAddress.Street1 =
          entities.PrivateAttorneyAddress.Street1;
        local.Attrony.Update.PrivateAttorneyAddress.Street2 =
          entities.PrivateAttorneyAddress.Street2;
        local.Attrony.Update.PrivateAttorneyAddress.City =
          entities.PrivateAttorneyAddress.City;
        local.Attrony.Update.PrivateAttorneyAddress.State =
          entities.PrivateAttorneyAddress.State;
        local.Attrony.Update.PrivateAttorneyAddress.ZipCode5 =
          entities.PrivateAttorneyAddress.ZipCode5;
      }
      while(!Equal(local.ReadEabFileHandling.Status, "OK"));

      if (ReadTribunalFips())
      {
        local.Tribunal.Assign(entities.Tribunal);
        local.Fips.CountyDescription = entities.Fips.CountyDescription;
      }
      else
      {
        local.Tribunal.DocumentHeader1 =
          Spaces(Tribunal.DocumentHeader1_MaxLength);
        local.Tribunal.DocumentHeader2 =
          Spaces(Tribunal.DocumentHeader2_MaxLength);
        local.Tribunal.DocumentHeader3 =
          Spaces(Tribunal.DocumentHeader3_MaxLength);
        local.Tribunal.DocumentHeader4 =
          Spaces(Tribunal.DocumentHeader4_MaxLength);
        local.Tribunal.DocumentHeader5 =
          Spaces(Tribunal.DocumentHeader5_MaxLength);
        local.Tribunal.DocumentHeader6 =
          Spaces(Tribunal.DocumentHeader6_MaxLength);
      }

      local.ReadLegalAction.Identifier = 0;

      foreach(var item in ReadCourtCaptionLegalAction())
      {
        if (entities.LegalAction.Identifier != local
          .ReadLegalAction.Identifier && local.ReadLegalAction.Identifier > 0)
        {
          break;
        }

        local.ReadLegalAction.Identifier = entities.LegalAction.Identifier;

        ++local.LegalCaptionGroupView.Index;
        local.LegalCaptionGroupView.CheckSize();

        MoveCourtCaption(entities.CourtCaption,
          local.LegalCaptionGroupView.Update.LegalCaption);
      }

      UseLeB608WriteEoaDocuments();
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseLeB608ReadExtractFile3();

    if (!Equal(local.Close.Status, "00"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the AR Statement Report files.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Close.Action = "CLOSE";
    UseSpEabWriteDocument2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveAp(Local.ApGroup source,
    LeB608WriteEoaDocuments.Import.GimportApNameGroup target)
  {
    target.Ap.FormattedName = source.Ap1.FormattedName;
  }

  private static void MoveAr(Local.ArGroup source,
    LeB608WriteEoaDocuments.Import.GimportArNameGroup target)
  {
    target.Ar.FormattedName = source.Ar1.FormattedName;
  }

  private static void MoveAttronyToGimportAttyName(Local.AttronyGroup source,
    LeB608WriteEoaDocuments.Import.GimportAttyNameGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.Atty.FormattedName = source.Attr.FormattedName;
    target.PrivateAttorneyAddress.Assign(source.PrivateAttorneyAddress);
  }

  private static void MoveCourtCaption(CourtCaption source, CourtCaption target)
  {
    target.Number = source.Number;
    target.Line = source.Line;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.County = source.County;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalCaptionGroupViewToGimportLegalCaption(Local.
    LegalCaptionGroupViewGroup source,
    LeB608WriteEoaDocuments.Import.GimportLegalCaptionGroup target)
  {
    MoveCourtCaption(source.LegalCaption, target.CourtCaption);
  }

  private static void MovePersonPrivateAttorney(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text12 = source.Text12;
    target.Text17 = source.Text17;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ReadCsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.Name.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseLeB608ReadExtractFile1()
  {
    var useImport = new LeB608ReadExtractFile.Import();
    var useExport = new LeB608ReadExtractFile.Export();

    useImport.EabFileHandling.Action = local.ReadEabFileHandling.Action;
    MoveFips(local.Fips, useExport.Fips);
    useExport.LeadAtty.Text2 = local.LeadAtty.Text2;
    MoveLegalAction(local.LegalAction, useExport.LegalAction);
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);
    useExport.AttorInfo.Text200 = local.AttorInfo.Text200;
    useExport.ApNumber.Text200 = local.ApNumber.Text200;
    useExport.ArNumber.Text200 = local.ArNumber.Text200;

    Call(LeB608ReadExtractFile.Execute, useImport, useExport);

    MoveFips(useExport.Fips, local.Fips);
    local.LeadAtty.Text2 = useExport.LeadAtty.Text2;
    MoveLegalAction(useExport.LegalAction, local.LegalAction);
    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
    local.AttorInfo.Text200 = useExport.AttorInfo.Text200;
    local.ApNumber.Text200 = useExport.ApNumber.Text200;
    local.ArNumber.Text200 = useExport.ArNumber.Text200;
  }

  private void UseLeB608ReadExtractFile2()
  {
    var useImport = new LeB608ReadExtractFile.Import();
    var useExport = new LeB608ReadExtractFile.Export();

    useImport.EabFileHandling.Action = local.Open.Action;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);

    Call(LeB608ReadExtractFile.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseLeB608ReadExtractFile3()
  {
    var useImport = new LeB608ReadExtractFile.Import();
    var useExport = new LeB608ReadExtractFile.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    MoveEabFileHandling(local.Close, useExport.EabFileHandling);

    Call(LeB608ReadExtractFile.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.Close);
  }

  private void UseLeB608WriteEoaDocuments()
  {
    var useImport = new LeB608WriteEoaDocuments.Import();
    var useExport = new LeB608WriteEoaDocuments.Export();

    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Tribunal.Assign(local.Tribunal);
    useImport.Fips.CountyDescription = local.Fips.CountyDescription;
    useImport.PrivateAttorneyAddress.Assign(local.Loacl);
    useImport.AttyCertificateNum.Text5 = local.AttryCertifiactionNum.Text5;
    MovePersonPrivateAttorney(local.PersonPrivateAttorney,
      useImport.PersonPrivateAttorney);
    local.LegalCaptionGroupView.CopyTo(
      useImport.GimportLegalCaption,
      MoveLegalCaptionGroupViewToGimportLegalCaption);
    local.Attrony.
      CopyTo(useImport.GimportAttyName, MoveAttronyToGimportAttyName);
    local.Ap.CopyTo(useImport.GimportApName, MoveAp);
    local.Ar.CopyTo(useImport.GimportArName, MoveAr);
    MoveWorkArea(local.AttyFaxPhone, useImport.AttyPhoneFax);
    useImport.OfficeName.Text37 = local.OfficeName.Text37;
    useImport.Att.EmailAddress = local.Att.EmailAddress;
    useImport.AttyTitle.Text17 = local.AttryTitle.Text17;

    Call(LeB608WriteEoaDocuments.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.Name);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.Name.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSpEabWriteDocument1()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabReportSend.Assign(local.Rpt);
    useImport.EabFileHandling.Action = local.Open.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabWriteDocument2()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.EabFileHandling.Status = local.Close.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCourtCaptionLegalAction()
  {
    entities.CourtCaption.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadCourtCaptionLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.CourtCaption.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPrivateAttorney()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", local.ReadPersonPrivateAttorney.Identifier);
        db.SetString(command, "cspNumber", local.ReadCsePersonsWorkSet.Number);
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 3);
        entities.PersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.Assert(entities.PersonPrivateAttorney.Populated);
    entities.PrivateAttorneyAddress.Populated = false;

    return Read("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier", entities.PersonPrivateAttorney.Identifier);
        db.SetString(
          command, "cspNumber", entities.PersonPrivateAttorney.CspNumber);
      },
      (db, reader) =>
      {
        entities.PrivateAttorneyAddress.PpaIdentifier = db.GetInt32(reader, 0);
        entities.PrivateAttorneyAddress.CspNumber = db.GetString(reader, 1);
        entities.PrivateAttorneyAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.PrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.PrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.PrivateAttorneyAddress.City = db.GetNullableString(reader, 5);
        entities.PrivateAttorneyAddress.State = db.GetNullableString(reader, 6);
        entities.PrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 7);
        entities.PrivateAttorneyAddress.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.DocumentHeader1 = db.GetNullableString(reader, 2);
        entities.Tribunal.DocumentHeader2 = db.GetNullableString(reader, 3);
        entities.Tribunal.DocumentHeader3 = db.GetNullableString(reader, 4);
        entities.Tribunal.DocumentHeader4 = db.GetNullableString(reader, 5);
        entities.Tribunal.DocumentHeader5 = db.GetNullableString(reader, 6);
        entities.Tribunal.DocumentHeader6 = db.GetNullableString(reader, 7);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 8);
        entities.Fips.County = db.GetInt32(reader, 8);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 9);
        entities.Fips.State = db.GetInt32(reader, 9);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 10);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalCaptionGroupViewGroup group.</summary>
    [Serializable]
    public class LegalCaptionGroupViewGroup
    {
      /// <summary>
      /// A value of LegalCaption.
      /// </summary>
      [JsonPropertyName("legalCaption")]
      public CourtCaption LegalCaption
      {
        get => legalCaption ??= new();
        set => legalCaption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CourtCaption legalCaption;
    }

    /// <summary>A AttronyGroup group.</summary>
    [Serializable]
    public class AttronyGroup
    {
      /// <summary>
      /// A value of PrivateAttorneyAddress.
      /// </summary>
      [JsonPropertyName("privateAttorneyAddress")]
      public PrivateAttorneyAddress PrivateAttorneyAddress
      {
        get => privateAttorneyAddress ??= new();
        set => privateAttorneyAddress = value;
      }

      /// <summary>
      /// A value of Attr.
      /// </summary>
      [JsonPropertyName("attr")]
      public CsePersonsWorkSet Attr
      {
        get => attr ??= new();
        set => attr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private PrivateAttorneyAddress privateAttorneyAddress;
      private CsePersonsWorkSet attr;
    }

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of Ap1.
      /// </summary>
      [JsonPropertyName("ap1")]
      public CsePersonsWorkSet Ap1
      {
        get => ap1 ??= new();
        set => ap1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private CsePersonsWorkSet ap1;
    }

    /// <summary>A ArGroup group.</summary>
    [Serializable]
    public class ArGroup
    {
      /// <summary>
      /// A value of Ar1.
      /// </summary>
      [JsonPropertyName("ar1")]
      public CsePersonsWorkSet Ar1
      {
        get => ar1 ??= new();
        set => ar1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private CsePersonsWorkSet ar1;
    }

    /// <summary>
    /// A value of AttyFaxPhone.
    /// </summary>
    [JsonPropertyName("attyFaxPhone")]
    public WorkArea AttyFaxPhone
    {
      get => attyFaxPhone ??= new();
      set => attyFaxPhone = value;
    }

    /// <summary>
    /// A value of OfficeName.
    /// </summary>
    [JsonPropertyName("officeName")]
    public WorkArea OfficeName
    {
      get => officeName ??= new();
      set => officeName = value;
    }

    /// <summary>
    /// A value of Att.
    /// </summary>
    [JsonPropertyName("att")]
    public ServiceProvider Att
    {
      get => att ??= new();
      set => att = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of ReadLegalAction.
    /// </summary>
    [JsonPropertyName("readLegalAction")]
    public LegalAction ReadLegalAction
    {
      get => readLegalAction ??= new();
      set => readLegalAction = value;
    }

    /// <summary>
    /// A value of Rpt.
    /// </summary>
    [JsonPropertyName("rpt")]
    public EabReportSend Rpt
    {
      get => rpt ??= new();
      set => rpt = value;
    }

    /// <summary>
    /// A value of ReadPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("readPersonPrivateAttorney")]
    public PersonPrivateAttorney ReadPersonPrivateAttorney
    {
      get => readPersonPrivateAttorney ??= new();
      set => readPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of ProcessName.
    /// </summary>
    [JsonPropertyName("processName")]
    public Common ProcessName
    {
      get => processName ??= new();
      set => processName = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of ReadCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("readCsePersonsWorkSet")]
    public CsePersonsWorkSet ReadCsePersonsWorkSet
    {
      get => readCsePersonsWorkSet ??= new();
      set => readCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public CsePerson ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// A value of AttyPhone.
    /// </summary>
    [JsonPropertyName("attyPhone")]
    public WorkArea AttyPhone
    {
      get => attyPhone ??= new();
      set => attyPhone = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Loacl.
    /// </summary>
    [JsonPropertyName("loacl")]
    public PrivateAttorneyAddress Loacl
    {
      get => loacl ??= new();
      set => loacl = value;
    }

    /// <summary>
    /// A value of AttryTitle.
    /// </summary>
    [JsonPropertyName("attryTitle")]
    public WorkArea AttryTitle
    {
      get => attryTitle ??= new();
      set => attryTitle = value;
    }

    /// <summary>
    /// A value of AttryCertifiactionNum.
    /// </summary>
    [JsonPropertyName("attryCertifiactionNum")]
    public WorkArea AttryCertifiactionNum
    {
      get => attryCertifiactionNum ??= new();
      set => attryCertifiactionNum = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// Gets a value of LegalCaptionGroupView.
    /// </summary>
    [JsonIgnore]
    public Array<LegalCaptionGroupViewGroup> LegalCaptionGroupView =>
      legalCaptionGroupView ??= new(LegalCaptionGroupViewGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalCaptionGroupView for json serialization.
    /// </summary>
    [JsonPropertyName("legalCaptionGroupView")]
    [Computed]
    public IList<LegalCaptionGroupViewGroup> LegalCaptionGroupView_Json
    {
      get => legalCaptionGroupView;
      set => LegalCaptionGroupView.Assign(value);
    }

    /// <summary>
    /// Gets a value of Attrony.
    /// </summary>
    [JsonIgnore]
    public Array<AttronyGroup> Attrony => attrony ??= new(
      AttronyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Attrony for json serialization.
    /// </summary>
    [JsonPropertyName("attrony")]
    [Computed]
    public IList<AttronyGroup> Attrony_Json
    {
      get => attrony;
      set => Attrony.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ar.
    /// </summary>
    [JsonIgnore]
    public Array<ArGroup> Ar => ar ??= new(ArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ar for json serialization.
    /// </summary>
    [JsonPropertyName("ar")]
    [Computed]
    public IList<ArGroup> Ar_Json
    {
      get => ar;
      set => Ar.Assign(value);
    }

    /// <summary>
    /// A value of ArNumber.
    /// </summary>
    [JsonPropertyName("arNumber")]
    public WorkArea ArNumber
    {
      get => arNumber ??= new();
      set => arNumber = value;
    }

    /// <summary>
    /// A value of ApNumber.
    /// </summary>
    [JsonPropertyName("apNumber")]
    public WorkArea ApNumber
    {
      get => apNumber ??= new();
      set => apNumber = value;
    }

    /// <summary>
    /// A value of AttorInfo.
    /// </summary>
    [JsonPropertyName("attorInfo")]
    public WorkArea AttorInfo
    {
      get => attorInfo ??= new();
      set => attorInfo = value;
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

    /// <summary>
    /// A value of LeadAtty.
    /// </summary>
    [JsonPropertyName("leadAtty")]
    public TextWorkArea LeadAtty
    {
      get => leadAtty ??= new();
      set => leadAtty = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabFileHandling Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of ReadEabFileHandling.
    /// </summary>
    [JsonPropertyName("readEabFileHandling")]
    public EabFileHandling ReadEabFileHandling
    {
      get => readEabFileHandling ??= new();
      set => readEabFileHandling = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private WorkArea attyFaxPhone;
    private WorkArea officeName;
    private ServiceProvider att;
    private CsePersonsWorkSet clear;
    private LegalAction readLegalAction;
    private EabReportSend rpt;
    private PersonPrivateAttorney readPersonPrivateAttorney;
    private Common processName;
    private CsePersonsWorkSet name;
    private CsePersonsWorkSet readCsePersonsWorkSet;
    private CsePerson readCsePerson;
    private WorkArea attyPhone;
    private EabFileHandling eabFileHandling;
    private Tribunal tribunal;
    private PrivateAttorneyAddress loacl;
    private WorkArea attryTitle;
    private WorkArea attryCertifiactionNum;
    private PersonPrivateAttorney personPrivateAttorney;
    private Array<LegalCaptionGroupViewGroup> legalCaptionGroupView;
    private Array<AttronyGroup> attrony;
    private Array<ApGroup> ap;
    private Array<ArGroup> ar;
    private WorkArea arNumber;
    private WorkArea apNumber;
    private WorkArea attorInfo;
    private LegalAction legalAction;
    private TextWorkArea leadAtty;
    private Fips fips;
    private WorkArea workArea;
    private EabReportSend eabReportSend;
    private EabFileHandling open;
    private EabFileHandling close;
    private EabFileHandling readEabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
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
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
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

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    private PrivateAttorneyAddress privateAttorneyAddress;
    private CsePerson csePerson;
    private CourtCaption courtCaption;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private PersonPrivateAttorney personPrivateAttorney;
  }
#endregion
}
