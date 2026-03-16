using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class ConfigurationService : IConfigurationService
    {
        public List<SelectListItem> GetRoles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Student" },
                new SelectListItem { Value = "2", Text = "Teacher" },
                new SelectListItem { Value = "3", Text = "Admin" },
                new SelectListItem { Value = "4", Text = "Assistant" },
                new SelectListItem { Value = "5", Text = "SuppAgent" },
                new SelectListItem { Value = "6", Text = "UniTeacher" },
                new SelectListItem { Value = "7", Text = "SuperAdmin" },
            };
        }

        public List<UniversityVM> GetUniversities()
        {
            return new List<UniversityVM>
            {
                new UniversityVM {InDDL=false,Value = 0, Text = "For All" },
                new UniversityVM {PackageID = null,Duration = null,InDDL=false,Value = 1,UrlAction="Register", Text = "First Aid Made Easy",CanCreateTest=true,AutoAccept=false },
                new UniversityVM {PackageID = GetProp("packID") , Duration = GetProp("duration"),  Value = 2, UrlAction="RegisterUni", Text = "ASMI" },
                new UniversityVM {PackageID = GetProp("packID4") ,Duration = GetProp("duration4"), Value = 4, UrlAction="AmbSubscription", Text = "ASMI Ambassidors" },
                new UniversityVM {PackageID = GetProp("packID") , Duration = GetProp("duration"),  Value = 3, UrlAction="FreeSubscription", Text = "ASMI Free",IsActive=false },
                new UniversityVM {PackageID = GetProp("packID5") ,Duration = GetProp("duration5"), Value = 5, UrlAction="RegisterUni5", Text = "ASMI Subscription",IsActive=false },
                new UniversityVM {PackageID = GetProp("packID6") ,Duration = GetProp("duration6"), Value = 6, UrlAction="Jalalabad", Text = "JASU Subscription",IsActive=false },
                new UniversityVM {PackageID = GetProp("packID7") ,Duration = GetProp("duration7"), Value = 7, UrlAction="MOCK_TEST_FOR_FCPS_NRE", Text = "Mock Test",AutoAccept=true },
                new UniversityVM {PackageID = GetProp("packID8") ,Duration = GetProp("duration8"), Value = 8, UrlAction="Mobile_App_Inauguration", Text = "Mobile App Inauguration",AutoAccept = true },
                new UniversityVM {PackageID = GetProp("packID9") ,Duration = GetProp("duration9"), Value = 9, UrlAction="Free_Plab", Text = "Free Plab 10 days",AutoAccept = true },
                new UniversityVM {PackageID = GetProp("packID10") ,Duration = GetProp("duration10"), Value = 10, UrlAction="FREE_NRE_MADE_EASY_6th", Text = "FREE NRE MADE EASY 6th EDITION",AutoAccept = true },
                new UniversityVM {PackageID = GetProp("packID11") ,Duration = GetProp("duration11"), Value = 11, UrlAction="Free_Nre_2", Text = "Free NRE-2 10 days",AutoAccept = false },
                new UniversityVM {PackageID = GetProp("packID12") ,Duration = GetProp("duration12"), Value = 12, UrlAction="Free_Nre_1", Text = "Free NRE-1 10 days",AutoAccept = false }
            };
        }

        public List<CountryVM> GetCountries()
        {
            return new List<CountryVM>()
            {
                new CountryVM{CodeS = "AF",Code="93",Name="Afghanistan (+93)"},
                new CountryVM{CodeS = "DZ" ,Code="213",Name="Algeria (+213)"},
                new CountryVM{CodeS = "AD" ,Code="376",Name="Andorra (+376)"},
                new CountryVM{CodeS = "AO" ,Code="244",Name="Angola (+244)"},
                new CountryVM{CodeS = "AI" ,Code="1264",Name="Anguilla (+1264)"},
                new CountryVM{CodeS = "AG" ,Code="1268",Name="Antigua & Barbuda(+1268)"},
                new CountryVM{CodeS = "AR" ,Code="54",Name="Argentina(+54)"},
                new CountryVM{CodeS = "AM" ,Code="374",Name="Armenia(+374)"},
                new CountryVM{CodeS = "AW" ,Code="297",Name="Aruba(+297)"},
                new CountryVM{CodeS = "AU" ,Code="61",Name="Australia(+61)"},
                new CountryVM{CodeS = "AT" ,Code="43",Name="Austria(+43)"},
                new CountryVM{CodeS = "AZ" ,Code="994",Name="Azerbaijan(+994)"},
                new CountryVM{CodeS = "BS" ,Code="1242",Name="Bahamas(+1242)"},
                new CountryVM{CodeS = "BH" ,Code="973",Name="Bahrain(+973)"},
                new CountryVM{CodeS = "BD" ,Code="880",Name="Bangladesh(+880)"},
                new CountryVM{CodeS = "BB" ,Code="1246",Name="Barbados(+1246)"},
                new CountryVM{CodeS = "BY" ,Code="375",Name="Belarus(+375)"},
                new CountryVM{CodeS = "BE" ,Code="32",Name="Belgium(+32)"},
                new CountryVM{CodeS = "BZ" ,Code="501",Name="Belize(+501)"},
                new CountryVM{CodeS = "BJ" ,Code="229",Name="Benin(+229)"},
                new CountryVM{CodeS = "BM" ,Code="1441",Name="Bermuda(+1441)"},
                new CountryVM{CodeS = "BT" ,Code="975",Name="Bhutan(+975)"},
                new CountryVM{CodeS = "BO" ,Code="591",Name="Bolivia(+591)"},
                new CountryVM{CodeS = "BA" ,Code="387",Name="Bosnia Herzegovina(+387)"},
                new CountryVM{CodeS = "BW" ,Code="267",Name="Botswana(+267)"},
                new CountryVM{CodeS = "BR" ,Code="55",Name="Brazil(+55)"},
                new CountryVM{CodeS = "BN" ,Code="673",Name="Brunei(+673)"},
                new CountryVM{CodeS = "BG" ,Code="359",Name="Bulgaria(+359)"},
                new CountryVM{CodeS = "BF" ,Code="226",Name="Burkina Faso(+226)"},
                new CountryVM{CodeS = "BI" ,Code="257",Name="Burundi(+257)"},
                new CountryVM{CodeS = "KH" ,Code="855",Name="Cambodia(+855)"},
                new CountryVM{CodeS = "CM" ,Code="237",Name="Cameroon(+237)"},
                new CountryVM{CodeS = "CA" ,Code="1",Name="Canada(+1)"},
                new CountryVM{CodeS = "CV" ,Code="238",Name="Cape Verde Islands(+238)"},
                new CountryVM{CodeS = "KY" ,Code="1345",Name="Cayman Islands(+1345)"},
                new CountryVM{CodeS = "CF" ,Code="236",Name="Central African Republic(+236)"},
                new CountryVM{CodeS = "CL" ,Code="56",Name="Chile(+56)"},
                new CountryVM{CodeS = "CN" ,Code="86",Name="China(+86)"},
                new CountryVM{CodeS = "CO" ,Code="57",Name="Colombia(+57)"},
                new CountryVM{CodeS = "KM" ,Code="269",Name="Comoros(+269)"},
                new CountryVM{CodeS = "CG" ,Code="242",Name="Congo(+242)"},
                new CountryVM{CodeS = "CK" ,Code="682",Name="Cook Islands(+682)"},
                new CountryVM{CodeS = "CR" ,Code="506",Name="Costa Rica(+506)"},
                new CountryVM{CodeS = "HR" ,Code="385",Name="Croatia(+385)"},
                new CountryVM{CodeS = "CU" ,Code="53",Name="Cuba(+53)"},
                new CountryVM{CodeS = "CY" ,Code="90392",Name="Cyprus North(+90392)"},
                new CountryVM{CodeS = "CY" ,Code="357",Name="Cyprus South(+357)"},
                new CountryVM{CodeS = "CZ" ,Code="42",Name="Czech Republic(+42)"},
                new CountryVM{CodeS = "DK" ,Code="45",Name="Denmark(+45)"},
                new CountryVM{CodeS = "DJ" ,Code="253",Name="Djibouti(+253)"},
                new CountryVM{CodeS = "DM" ,Code="1809",Name="Dominica(+1809)"},
                new CountryVM{CodeS = "DO" ,Code="1809",Name="Dominican Republic(+1809)"},
                new CountryVM{CodeS = "EC" ,Code="593",Name="Ecuador(+593)"},
                new CountryVM{CodeS = "EG" ,Code="20",Name="Egypt(+20)"},
                new CountryVM{CodeS = "SV" ,Code="503",Name="El Salvador(+503)"},
                new CountryVM{CodeS = "GQ" ,Code="240",Name="Equatorial Guinea(+240)"},
                new CountryVM{CodeS = "ER" ,Code="291",Name="Eritrea(+291)"},
                new CountryVM{CodeS = "EE" ,Code="372",Name="Estonia(+372)"},
                new CountryVM{CodeS = "ET" ,Code="251",Name="Ethiopia(+251)"},
                new CountryVM{CodeS = "FK" ,Code="500",Name="Falkland Islands(+500)"},
                new CountryVM{CodeS = "FO" ,Code="298",Name="Faroe Islands(+298)"},
                new CountryVM{CodeS = "FJ" ,Code="679",Name="Fiji(+679)"},
                new CountryVM{CodeS = "FI" ,Code="358",Name="Finland(+358)"},
                new CountryVM{CodeS = "FR" ,Code="33",Name="France(+33)"},
                new CountryVM{CodeS = "GF" ,Code="594",Name="French Guiana(+594)"},
                new CountryVM{CodeS = "PF" ,Code="689",Name="French Polynesia(+689)"},
                new CountryVM{CodeS = "GA" ,Code="241",Name="Gabon(+241)"},
                new CountryVM{CodeS = "GM" ,Code="220",Name="Gambia(+220)"},
                new CountryVM{CodeS = "GE" ,Code="7880",Name="Georgia(+7880)"},
                new CountryVM{CodeS = "DE" ,Code="49",Name="Germany(+49)"},
                new CountryVM{CodeS = "GH" ,Code="233",Name="Ghana(+233)"},
                new CountryVM{CodeS = "GI" ,Code="350",Name="Gibraltar(+350)"},
                new CountryVM{CodeS = "GR" ,Code="30",Name="Greece(+30)"},
                new CountryVM{CodeS = "GL" ,Code="299",Name="Greenland(+299)"},
                new CountryVM{CodeS = "GD" ,Code="1473",Name="Grenada(+1473)"},
                new CountryVM{CodeS = "GP" ,Code="590",Name="Guadeloupe(+590)"},
                new CountryVM{CodeS = "GU" ,Code="671",Name="Guam(+671)"},
                new CountryVM{CodeS = "GT" ,Code="502",Name="Guatemala(+502)"},
                new CountryVM{CodeS = "GN" ,Code="224",Name="Guinea(+224)"},
                new CountryVM{CodeS = "GW" ,Code="245",Name="Guinea - Bissau(+245)"},
                new CountryVM{CodeS = "GY" ,Code="592",Name="Guyana(+592)"},
                new CountryVM{CodeS = "HT" ,Code="509",Name="Haiti(+509)"},
                new CountryVM{CodeS = "HN" ,Code="504",Name="Honduras(+504)"},
                new CountryVM{CodeS = "HK" ,Code="852",Name="Hong Kong(+852)"},
                new CountryVM{CodeS = "HU" ,Code="36",Name="Hungary(+36)"},
                new CountryVM{CodeS = "IS" ,Code="354",Name="Iceland(+354)"},
                new CountryVM{CodeS = "IN" ,Code="91",Name="India(+91)"},
                new CountryVM{CodeS = "ID" ,Code="62",Name="Indonesia(+62)"},
                new CountryVM{CodeS = "IR" ,Code="98",Name="Iran(+98)"},
                new CountryVM{CodeS = "IQ" ,Code="964",Name="Iraq(+964)"},
                new CountryVM{CodeS = "IE" ,Code="353",Name="Ireland(+353)"},
                new CountryVM{CodeS = "IL" ,Code="972",Name="Israel(+972)"},
                new CountryVM{CodeS = "IT" ,Code="39",Name="Italy(+39)"},
                new CountryVM{CodeS = "JM" ,Code="1876",Name="Jamaica(+1876)"},
                new CountryVM{CodeS = "JP" ,Code="81",Name="Japan(+81)"},
                new CountryVM{CodeS = "JO" ,Code="962",Name="Jordan(+962)"},
                new CountryVM{CodeS = "KZ" ,Code="7",Name="Kazakhstan(+7)"},
                new CountryVM{CodeS = "KE" ,Code="254",Name="Kenya(+254)"},
                new CountryVM{CodeS = "KI" ,Code="686",Name="Kiribati(+686)"},
                new CountryVM{CodeS = "KP" ,Code="850",Name="Korea North(+850)"},
                new CountryVM{CodeS = "KR" ,Code="82",Name="Korea South(+82)"},
                new CountryVM{CodeS = "KW" ,Code="965",Name="Kuwait(+965)"},
                new CountryVM{CodeS = "KG" ,Code="996",Name="Kyrgyzstan(+996)"},
                new CountryVM{CodeS = "LA" ,Code="856",Name="Laos(+856)"},
                new CountryVM{CodeS = "LV" ,Code="371",Name="Latvia(+371)"},
                new CountryVM{CodeS = "LB" ,Code="961",Name="Lebanon(+961)"},
                new CountryVM{CodeS = "LS" ,Code="266",Name="Lesotho(+266)"},
                new CountryVM{CodeS = "LR" ,Code="231",Name="Liberia(+231)"},
                new CountryVM{CodeS = "LY" ,Code="218",Name="Libya(+218)"},
                new CountryVM{CodeS = "LI" ,Code="417",Name="Liechtenstein(+417)"},
                new CountryVM{CodeS = "LT" ,Code="370",Name="Lithuania(+370)"},
                new CountryVM{CodeS = "LU" ,Code="352",Name="Luxembourg(+352)"},
                new CountryVM{CodeS = "MO" ,Code="853",Name="Macao(+853)"},
                new CountryVM{CodeS = "MK" ,Code="389",Name="Macedonia(+389)"},
                new CountryVM{CodeS = "MG" ,Code="261",Name="Madagascar(+261)"},
                new CountryVM{CodeS = "MW" ,Code="265",Name="Malawi(+265)"},
                new CountryVM{CodeS = "MY" ,Code="60",Name="Malaysia(+60)"},
                new CountryVM{CodeS = "MV" ,Code="960",Name="Maldives(+960)"},
                new CountryVM{CodeS = "ML" ,Code="223",Name="Mali(+223)"},
                new CountryVM{CodeS = "MT" ,Code="356",Name="Malta(+356)"},
                new CountryVM{CodeS = "MH" ,Code="692",Name="Marshall Islands(+692)"},
                new CountryVM{CodeS = "MQ" ,Code="596",Name="Martinique(+596)"},
                new CountryVM{CodeS = "MR" ,Code="222",Name="Mauritania(+222)"},
                new CountryVM{CodeS = "YT" ,Code="269",Name="Mayotte(+269)"},
                new CountryVM{CodeS = "MX" ,Code="52",Name="Mexico(+52)"},
                new CountryVM{CodeS = "FM" ,Code="691",Name="Micronesia(+691)"},
                new CountryVM{CodeS = "MD" ,Code="373",Name="Moldova(+373)"},
                new CountryVM{CodeS = "MC" ,Code="377",Name="Monaco(+377)"},
                new CountryVM{CodeS = "MN" ,Code="976",Name="Mongolia(+976)"},
                new CountryVM{CodeS = "MS" ,Code="1664",Name="Montserrat(+1664)"},
                new CountryVM{CodeS = "MA" ,Code="212",Name="Morocco(+212)"},
                new CountryVM{CodeS = "MZ" ,Code="258",Name="Mozambique(+258)"},
                new CountryVM{CodeS = "MN" ,Code="95",Name="Myanmar(+95)"},
                new CountryVM{CodeS = "NA" ,Code="264",Name="Namibia(+264)"},
                new CountryVM{CodeS = "NR" ,Code="674",Name="Nauru(+674)"},
                new CountryVM{CodeS = "NP" ,Code="977",Name="Nepal(+977)"},
                new CountryVM{CodeS = "NL" ,Code="31",Name="Netherlands(+31)"},
                new CountryVM{CodeS = "NC" ,Code="687",Name="New Caledonia(+687)"},
                new CountryVM{CodeS = "NZ" ,Code="64",Name="New Zealand(+64)"},
                new CountryVM{CodeS = "NI" ,Code="505",Name="Nicaragua(+505)"},
                new CountryVM{CodeS = "NE" ,Code="227",Name="Niger(+227)"},
                new CountryVM{CodeS = "NG" ,Code="234",Name="Nigeria(+234)"},
                new CountryVM{CodeS = "NU" ,Code="683",Name="Niue(+683)"},
                new CountryVM{CodeS = "NF" ,Code="672",Name="Norfolk Islands(+672)"},
                new CountryVM{CodeS = "NP" ,Code="670",Name="Northern Marianas(+670)"},
                new CountryVM{CodeS = "NO" ,Code="47",Name="Norway(+47)"},
                new CountryVM{CodeS = "OM" ,Code="968",Name="Oman(+968)"},
                new CountryVM{CodeS = "PW" ,Code="680",Name="Palau(+680)"},
                new CountryVM{CodeS = "PA" ,Code="507",Name="Panama(+507)"},
                new CountryVM{CodeS = "PG" ,Code="675",Name="Papua New Guinea(+675)"},
                new CountryVM{CodeS = "PK" ,Code="92",Name="Pakistan(+92)"},
                new CountryVM{CodeS = "PY" ,Code="595",Name="Paraguay(+595)"},
                new CountryVM{CodeS = "PE" ,Code="51",Name="Peru(+51)"},
                new CountryVM{CodeS = "PH" ,Code="63",Name="Philippines(+63)"},
                new CountryVM{CodeS = "PL" ,Code="48",Name="Poland(+48)"},
                new CountryVM{CodeS = "PT" ,Code="351",Name="Portugal(+351)"},
                new CountryVM{CodeS = "PR" ,Code="1787",Name="Puerto Rico(+1787)"},
                new CountryVM{CodeS = "QA" ,Code="974",Name="Qatar(+974)"},
                new CountryVM{CodeS = "RE" ,Code="262",Name="Reunion(+262)"},
                new CountryVM{CodeS = "RO" ,Code="40",Name="Romania(+40)"},
                new CountryVM{CodeS = "RU" ,Code="7",Name="Russia(+7)"},
                new CountryVM{CodeS = "RW" ,Code="250",Name="Rwanda(+250)"},
                new CountryVM{CodeS = "SM" ,Code="378",Name="San Marino(+378)"},
                new CountryVM{CodeS = "ST" ,Code="239",Name="Sao Tome & Principe(+239)"},
                new CountryVM{CodeS = "SA" ,Code="966",Name="Saudi Arabia(+966)"},
                new CountryVM{CodeS = "SN" ,Code="221",Name="Senegal(+221)"},
                new CountryVM{CodeS = "CS" ,Code="381",Name="Serbia(+381)"},
                new CountryVM{CodeS = "SC" ,Code="248",Name="Seychelles(+248)"},
                new CountryVM{CodeS = "SL" ,Code="232",Name="Sierra Leone(+232)"},
                new CountryVM{CodeS = "SG" ,Code="65",Name="Singapore(+65)"},
                new CountryVM{CodeS = "SK" ,Code="421",Name="Slovak Republic(+421)"},
                new CountryVM{CodeS = "SI" ,Code="386",Name="Slovenia(+386)"},
                new CountryVM{CodeS = "SB" ,Code="677",Name="Solomon Islands(+677)"},
                new CountryVM{CodeS = "SO" ,Code="252",Name="Somalia(+252)"},
                new CountryVM{CodeS = "ZA" ,Code="27",Name="South Africa(+27)"},
                new CountryVM{CodeS = "ES" ,Code="34",Name="Spain(+34)"},
                new CountryVM{CodeS = "LK" ,Code="94",Name="Sri Lanka(+94)"},
                new CountryVM{CodeS = "SH" ,Code="290",Name="St.Helena(+290)"},
                new CountryVM{CodeS = "KN" ,Code="1869",Name="St.Kitts(+1869)"},
                new CountryVM{CodeS = "SC" ,Code="1758",Name="St.Lucia(+1758)"},
                new CountryVM{CodeS = "SD" ,Code="249",Name="Sudan(+249)"},
                new CountryVM{CodeS = "SR" ,Code="597",Name="Suriname(+597)"},
                new CountryVM{CodeS = "SZ" ,Code="268",Name="Swaziland(+268)"},
                new CountryVM{CodeS = "SE" ,Code="46",Name="Sweden(+46)"},
                new CountryVM{CodeS = "CH" ,Code="41",Name="Switzerland(+41)"},
                new CountryVM{CodeS = "SI" ,Code="963",Name="Syria(+963)"},
                new CountryVM{CodeS = "TW" ,Code="886",Name="Taiwan(+886)"},
                new CountryVM{CodeS = "TJ" ,Code="7",Name="Tajikstan(+7)"},
                new CountryVM{CodeS = "TH" ,Code="66",Name="Thailand(+66)"},
                new CountryVM{CodeS = "TG" ,Code="228",Name="Togo(+228)"},
                new CountryVM{CodeS = "TO" ,Code="676",Name="Tonga(+676)"},
                new CountryVM{CodeS = "TT" ,Code="1868",Name="Trinidad & Tobago(+1868)"},
                new CountryVM{CodeS = "TN" ,Code="216",Name="Tunisia(+216)"},
                new CountryVM{CodeS = "TR" ,Code="90",Name="Turkey(+90)"},
                new CountryVM{CodeS = "TM" ,Code="7",Name="Turkmenistan(+7)"},
                new CountryVM{CodeS = "TM" ,Code="993",Name="Turkmenistan(+993)"},
                new CountryVM{CodeS = "TC" ,Code="1649",Name="Turks & Caicos Islands(+1649)"},
                new CountryVM{CodeS = "TV" ,Code="688",Name="Tuvalu(+688)"},
                new CountryVM{CodeS = "UG" ,Code="256",Name="Uganda(+256)"},
                new CountryVM{CodeS = "GB" ,Code="44",Name="UK(+44)"},
                new CountryVM{CodeS = "UA" ,Code="380",Name="Ukraine(+380)"},
                new CountryVM{CodeS = "AE" ,Code="971",Name="United Arab Emirates(+971)"},
                new CountryVM{CodeS = "UY" ,Code="598",Name="Uruguay(+598)"},
                new CountryVM{CodeS = "US" ,Code="1",Name="USA(+1)"},
                new CountryVM{CodeS = "UZ" ,Code="7",Name="Uzbekistan(+7)"},
                new CountryVM{CodeS = "VU" ,Code="678",Name="Vanuatu(+678)"},
                new CountryVM{CodeS = "VA" ,Code="379",Name="Vatican City(+379)"},
                new CountryVM{CodeS = "VE" ,Code="58",Name="Venezuela(+58)"},
                new CountryVM{CodeS = "VN" ,Code="84",Name="Vietnam(+84)"},
                new CountryVM{CodeS = "VG" ,Code="84",Name="Virgin Islands - British(+1284)"},
                new CountryVM{CodeS = "VI" ,Code="84",Name="Virgin Islands - US(+1340)"},
                new CountryVM{CodeS = "WF" ,Code="681",Name="Wallis & Futuna(+681)"},
                new CountryVM{CodeS = "YE" ,Code="969",Name="Yemen(North)(+969)"},
                new CountryVM{CodeS = "YE" ,Code="967",Name="Yemen(South)(+967)"},
                new CountryVM{CodeS = "ZM" ,Code="260",Name="Zambia(+260)"},
                new CountryVM{CodeS = "ZW" ,Code="263",Name="Zimbabwe(+263)"},
            };
        }

        public List<SelectListItem> GetDifficultyList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value="1", Text="Easy"},
                new SelectListItem { Value="2", Text="Medium"},
                new SelectListItem { Value="3", Text="Hard"},
            };
        }

        public List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value="Open", Text="Open"},
                new SelectListItem { Value="Close", Text="Close"},
            };
        }

        public List<SelectListItem> GetDurationList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value="3", Text="3 Days"},
                new SelectListItem { Value="4", Text="4 Days"},
                new SelectListItem { Value="5", Text="5 Days"},
                new SelectListItem { Value="6", Text="6 Days"},
                new SelectListItem { Value="7", Text="7 Days"},
                new SelectListItem { Value="8", Text="8 Days"},
                new SelectListItem { Value="9", Text="9 Days"},
                new SelectListItem { Value="10", Text="10 Days"},
                new SelectListItem { Value="15", Text="15 Days"},
                new SelectListItem { Value="20", Text="20 Days"},
                new SelectListItem { Value="45", Text="45 Days"},
                new SelectListItem { Value="30", Text="1 Month"},
                new SelectListItem { Value="60", Text="2 Month"},
                new SelectListItem { Value="90", Text="3 Month"},
                new SelectListItem { Value="150", Text="5 Month"},
                new SelectListItem { Value="180", Text="6 Month"},
                new SelectListItem { Value="360", Text="1 Year"},
                new SelectListItem { Value="720", Text="2 Year"},
            };
        }

        public List<SelectListItem> GetDurationInstList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value="0", Text="1st Day"},
                new SelectListItem { Value="30", Text="1 Month"},
                new SelectListItem { Value="60", Text="2 Month"},
                new SelectListItem { Value="90", Text="3 Month"},
                new SelectListItem { Value="120", Text="4 Month"},
                new SelectListItem { Value="150", Text="5 Month"},
                new SelectListItem { Value="180", Text="6 Month"},
                new SelectListItem { Value="360", Text="1 Year"},
                new SelectListItem { Value="720", Text="2 Year"},
            };
        }

        public List<SelectListItem> GetExamTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "NRE", Text = "NRE" },
                new SelectListItem { Value = "FCPS1", Text = "FCPS-1" }
            };
        }

        public List<SelectListItem> GetMockTestTypes()
        {
            return Enum.GetValues(typeof(MockTestTypes)).Cast<MockTestTypes>()
                .Select(e => new SelectListItem { Text = Common.GetDisplayName(e), Value = ((int)e).ToString() }).ToList();
        }

        private int? GetProp(string Name)
        {
            var v = WebConfigurationManager.AppSettings[Name];
            if (v == null) return null;
            return Convert.ToInt32(v);
        }
    }
}
