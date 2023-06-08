using System;
using Newtonsoft.Json;
using StyexFleetManagement.Resx;

namespace StyexFleetManagement.Models
{
    public class DriverData
    {
        public int DataType { get; set; }
        public string Data { get; set; }
        public string DriverId { get; set; }
        public int LicenseType { get; set; }
        public string LicenseId { get; set; }
        public int ExpiryDateYear { get; set; }
        public int ExpiryDateMonth { get; set; }
        public int BirthDateCentury { get; set; }
        public int BirthDateYear { get; set; }
        public int BirthDateMonth { get; set; }
        public int BirthDateDay { get; set; }
        public int PersonalCard { get; set; }
        public int Gender { get; set; }
        [JsonIgnore]
        public string LicenseString
        {
            get
            {
                switch ((LicenseType)this.LicenseType) {
                    case (Models.LicenseType.B1):
                        return $"{this.LicenseType.ToString()} ({"B1"})";
                    case (Models.LicenseType.B2):
                        return $"{this.LicenseType.ToString()} ({"B2"})";
                    case (Models.LicenseType.B3):
                        return $"{this.LicenseType.ToString()} ({"B3"})";
                    case (Models.LicenseType.B4):
                        return $"{this.LicenseType.ToString()} ({"B4"})";
                    case (Models.LicenseType.T1):
                        return $"{this.LicenseType.ToString()} ({"T1"})";
                    case (Models.LicenseType.T2):
                        return $"{this.LicenseType.ToString()} ({"T2"})";
                    case (Models.LicenseType.T3):
                        return $"{this.LicenseType.ToString()} ({"T3"})";
                    case (Models.LicenseType.T4):
                        return $"{this.LicenseType.ToString()} ({"T4"})";
                    default:
                        return $"{this.LicenseType.ToString()} ({"Pvt"})";
                }
            }
        }
        [JsonIgnore]
        public string GenderString
        {
            get
            {
                switch ((GenderEnum)this.Gender)
                {
                    case (GenderEnum.Male):
                        return AppResources.male;
                    case (GenderEnum.Female):
                        return AppResources.female;
                    default:
                        return "N/A";
                }
            }
        }

        public int CountryCode { get; set; }
        public int LicenceYear { get; set; }
        public string TransportOffice { get; set; }
        public string AreaOfLicenceRegistration { get; set; }

        [JsonIgnore]
        public DateTime BirthDate {
            get
            {
                try
                {
                    var year = int.Parse(BirthDateCentury.ToString() + BirthDateYear.ToString());
                    if (BirthDateDay<=31 && BirthDateMonth <= 12)
                    {
                        var date = new DateTime(year, (int)BirthDateMonth, (int)BirthDateDay);
                        return date;
                    }
                    else
                    {
                        return default(DateTime);
                    }
                }
                catch (ArgumentException)
                {
                    return default(DateTime);
                }
                catch (NullReferenceException)
                {
                    return default(DateTime);
                }
            }
            
        }

        [JsonIgnore]
        public string BirthDateString => BirthDate.ToString("dd/mm/yyyy");

        [JsonIgnore]
        public string LicenceExpiraryDateString => LicenceExpiraryDate.ToString("dd/mm/yyyy");

        [JsonIgnore]
        public DateTime LicenceExpiraryDate {
            get
            {
                try
                {
                    var year = int.Parse("20" + ExpiryDateYear.ToString());
                    if (ExpiryDateMonth <= 12)
                    {
                        var date = new DateTime(year, (int)ExpiryDateMonth, 1);
                        return date;
                    }
                    else
                    {
                        return default(DateTime);
                    }
                }
                catch (ArgumentException)
                {
                    return default(DateTime);
                }
                catch (NullReferenceException)
                {
                    return default(DateTime);
                }
            }
        }


        [JsonIgnore]
        public int NumberOfTrips { get; set; }
        [JsonIgnore]
        public string Vehicle { get; internal set; }
    }

    public enum GenderEnum
    {
        Male = 1,
        Female = 2
    }
}