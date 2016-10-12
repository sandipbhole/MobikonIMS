
$(document).ready(function ()
{    
    $("#countryName").change(function () {
        if ($("#countryName").val() != "" && $("#countryName").val() != "All" && $("#countryName").val() != "Select country") {
            var CountryOptions = {};
            CountryOptions.url = "/MobikonIMS/Client/GetCountrywiseCity";
            CountryOptions.type = "POST";
            CountryOptions.data = JSON.stringify({ countryName: $("#countryName").val() });
            CountryOptions.datatype = "json";
            CountryOptions.contentType = "application/json";
            CountryOptions.success = function (CityList)
            {
                $("#cityName").empty();
                $("#countryID").val(0)             

                for (var i = 0; i < CityList.length; i++)
                {
                    $("#countryID").val(CityList[i].countryID);
                    $("#cityName").append("<option>" + CityList[i].cityName + "</option>");
                }

                if (CityList.length == 0)
                {
                    $("#cityName").empty();
                    $("#countryID").val(0)
                    $("#cityName").append("<option> Select city </option>");
                }
                else
                {
                    $("#cityName").append("<option> Select city </option>");
                }
            };
            CountryOptions.error = function () { alert("Error in Getting Cities!!"); };
            $.ajax(CountryOptions);
        }
        else if ($("#countryName").val() == "All") {
            $("#cityName").empty();
            $("#countryID").val(0)
            //$("#cityName").prop("disabled", false);
            $("#cityName").append("<option> All </option>");
        }
        else if ($("#countryName").val() == "" || $("#countryName").val() == "Select country") {
            $("#cityName").empty();
            $("#countryID").val(0)
            //$("#cityName").prop("disabled", false);
            $("#cityName").append("<option> Select city </option>");
        }       
    });
});

$(document).ready(function () {
    //$("#cityNameSearch").prop("disabled", true);
    $("#countryNameSearch").change(function () {
        if ($("#countryNameSearch").val() != "All" && $("#countryNameSearch").val() != "Select country") {
            var CountryOptions = {};
            CountryOptions.url = "/MobikonIMS/Client/GetCountrywiseCity";
            CountryOptions.type = "POST";
            CountryOptions.data = JSON.stringify({ countryName: $("#countryNameSearch").val() });
            CountryOptions.datatype = "json";
            CountryOptions.contentType = "application/json";
            CountryOptions.success = function (StatesList) {
                $("#cityNameSearch").empty();
                $("#cityNameSearch").append("<option> All </option>");
                for (var i = 0; i < StatesList.length; i++) {
                    $("#cityNameSearch").append("<option>" + StatesList[i].cityName + "</option>");
                }
                if (StatesList.length >= 1) {
                    $("#cityNameSearch").prop("disabled", false);
                }
                else {
                    $("#cityNameSearch").prop("disabled", true);
                }
            };
            CountryOptions.error = function () { alert("Error in Getting Cities!!"); };
            $.ajax(CountryOptions);
        }
        else if ($("#countryNameSearch").val() == "All") {
            $("#cityNameSearch").empty();
            $("#cityNameSearch").prop("disabled", false);
            $("#cityNameSearch").append("<option> All </option>");
        }
        else if ($("#countryNameSearch").val() == "Select country") {
            $("#cityNameSearch").empty();
            $("#cityNameSearch").prop("disabled", false);
            $("#cityNameSearch").append("<option> Select city </option>");
        }       
    });
});