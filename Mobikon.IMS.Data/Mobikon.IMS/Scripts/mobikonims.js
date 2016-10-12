$(document).ready(function () {
    $("#productSerialSearch").change(function () {
         if ($("#productSerialSearch").val() == "" || $("#productSerialSearch").val() == "All")
        {
            $("#deviceDetailsSearch").val("");
            $("#deviceDetailsSearch").prop("readonly", false);
        }
        else
        {
            var DeviceOptions = {};
            DeviceOptions.url = "/MobikonIMS/Device/GetProductSerialwiseDevice";
            DeviceOptions.type = "POST";
            DeviceOptions.data = JSON.stringify({ productSerial: $("#productSerialSearch").val() });
            DeviceOptions.datatype = "json";
            DeviceOptions.contentType = "application/json";
            DeviceOptions.success = function (DeviceList) {
                //for (var i = 0; i < DeviceList.length; i++) {
                //$("#deviceDetails").append("<option>" + DeviceList[i].deviceDetails + "</option>");
                //$("#deviceID").val(DeviceList.deviceID);
                $("#deviceDetailsSearch").val(DeviceList.deviceDetails);
               
                $("#deviceDetailsSearch").prop("readonly", true);
                //$("#status").val(DeviceList.status);
                //$("#note").val(DeviceList.note);
                //  $("#companyOwner").val(ProductList.companyOwner);
                //}               
            };
            DeviceOptions.error = function () { alert("Error in Getting Device Details!!"); };
            $.ajax(DeviceOptions);
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
        //else
        //{
        //    $("#State").empty();
        //    $("#State").prop("disabled", true);
        //}
    });
});

$(document).ready(function () {
    $("#clientNameSearch").change(function () {
        if ($("#clientNameSearch").val() != "" && $("#clientNameSearch").val() != "All")
        {
            $("#outletNameSearch").empty();            
            $("#outletID").val(0);
            $("#outletNameSearch").append("<option>All</option>");

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetClientwiseOutletName";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ clientName: $("#clientNameSearch").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList)
            {
                for (var i = 0; i < OutletList.length; i++)
                {
                    $("#outletID").val(OutletList[i].outletID);
                    $("#outletNameSearch").append("<option>" + OutletList[i].outletName + "</option>");
                }
            };
            OutletOptions.error = function () { alert("Error in Getting Client Details!!"); };
            $.ajax(OutletOptions);
        }
        else {
            $("#outletNameSearch").empty();           
            $("#outletID").val(0);
            $("#outletNameSearch").append("<option>All</option>");
        }
    });
});

$(document).ready(function () {
    $("#productSerial").change(function () {
        if ($("#productSerial").val() != "" && $("#productSerial").val() != "Select device serial") {
            var ProductOptions = {};
            ProductOptions.url = "/MobikonIMS/Device/GetProductSerialwiseDevice";
            ProductOptions.type = "POST";
            ProductOptions.data = JSON.stringify({ productSerial: $("#productSerial").val() });
            ProductOptions.datatype = "json";
            ProductOptions.contentType = "application/json";
            ProductOptions.success = function (ProductList)
            {               
                $("#deviceID").val(ProductList.deviceID);
                $("#deviceDetails").val(ProductList.deviceDetails);                
                $("#status").val(ProductList.status);
                $("#deviceTag").val(ProductList.deviceTag);
                if (ProductList.status == "Blocked")
                {                  
                    $("#command").prop("disabled", true);
                }
                else
                {                   
                    $("#command").prop("disabled", false);
                }
                $("#companyOwner").val(ProductList.companyOwner);
                $("#note").val(ProductList.note);
            };
            ProductOptions.error = function () { alert("Error in Getting Device Details!!"); };
            $.ajax(ProductOptions);
        }
        else
        {
            $("#deviceID").val(0);
            $("#deviceDetails").val("");
            $("#companyOwner").val("Yes");
            $("#note").val("");
            $("#deviceTag").val("");
        }
    });
});

$(document).ready(function () {
   
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
                $("#cityName").append("<option> Select city </option>");
                $("#countryID").val(0)
                if (CityList.Length > 0)
                {
                    $("#cityName").prop("disabled", false);
                }
                else
                {
                    $("#cityName").prop("disabled", true);
                }

                for (var i = 0; i < CityList.length; i++)
                {
                    $("#countryID").val(CityList[i].countryID);
                    $("#cityName").append("<option>" + CityList[i].cityName + "</option>");
                }                
            };
            CountryOptions.error = function () { alert("Error in Getting Cities!!"); };
            $.ajax(CountryOptions);
        }
        else if ($("#countryName").val() == "All")
        {
            $("#cityName").empty();
            $("#countryID").val(0)
            //$("#cityName").prop("disabled", false);
            $("#cityName").append("<option> All </option>");
        }
        else if ($("#countryName").val() == "" || $("#countryName").val() == "Select country") {
            $("#cityName").empty();
            $("#countryID").val(0);           
            $("#cityName").append("<option> Select city </option>");
        }       
    });
});

$(document).ready(function () {
    $(function () { // will trigger when the document is ready
        $('.datepicker').datepicker({
            autoclose: true,
            format: 'dd/mm/yyyy',
            todayBtn: true,
            todayHighlight: true,
        }); //Initialise any date pickers
    });
});

$(document).ready(function () {
    $("#clientName").change(function ()
    {      
        if ($("#clientName").val() != "" && $("#clientName").val() != "Select brand name") {
            $("#outletName").empty();
            $("#address").val("");
            $("#cityID").val(0);
            $("#cityName").val("Select city");
            $("#countryID").val(0);
            $("#countryName").val("Select country");
            $("#outletID").val(0);
            $("#outletName").val("Select outlet");     

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetClientwiseOutletName";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ selectedClientName: $("#clientName").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList) {
                for (var i = 0; i < OutletList.length; i++) {
                    $("#address").val(OutletList[i].clientAddress);
                    $("#cityID").val(OutletList[i].clientCityID);
                    $("#cityName").val(OutletList[i].clientCity);
                    $("#countryID").val(OutletList[i].clientCountryID);
                    $("#countryName").val(OutletList[i].clientCountry);
                    $("#outletID").val(OutletList[i].outletID);                  
                    $("#outletName").append("<option>" + OutletList[i].outletName + "</option>");
                }
            };
            OutletOptions.error = function () { alert("Error in Getting Client Details!!"); };
            $.ajax(OutletOptions);
        }
        else
        {
            $("#outletName").empty();
            $("#address").val("");
            $("#cityID").val(0);
            $("#cityName").val("Select city");
            $("#countryID").val(0);
            $("#countryName").val("Select country");
            $("#outletID").val(0);
            $("#outletName").val("Select outlet");           
        }
    });
});

$(document).ready(function () {
    $("#outletName").change(function () {
        if ($("#outletName").val() != "Select outlet")
        {
            $("#outletID").val(0);
            $("#address").val("");
            $("#countryID").val(0);
            $("#countryName").val("Select country");
            $("#cityID").val(0); 

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetOutletnamewiseOutlet";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ outletName: $("#outletName").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList) {
                $("#outletID").val(OutletList.clientID);
                $("#address").val(OutletList.address);
                $("#countryID").val(OutletList.clientCountryID);
                $("#countryName").val(OutletList.countryName);
                $("#cityID").val(OutletList.clientCityID);
                $("#cityName").val(OutletList.cityName);
               
            };
            OutletOptions.error = function (e) {
                alert("Error in Getting Device Details!!");
            };
            $.ajax(OutletOptions);
        }
        else
        {
            $("#outletID").val(0);
            $("#address").val("");
            $("#countryID").val(0);
            $("#countryName").val("Select country");
            $("#cityName").val("Select city");
            $("#cityID").val(0);
        }
    });
});

$(document).ready(function () {
    $("#status").change(function () {
        if ($("#status").val() != "Select status") {
            $("#statusID").val(0);

            var StatusOptions = {};
            StatusOptions.url = "/MobikonIMS/Outlet/GetStatusnamewiseStatus";
            StatusOptions.type = "POST";
            StatusOptions.data = JSON.stringify({ statusName: $("#status").val() });
            StatusOptions.datatype = "json";
            StatusOptions.contentType = "application/json";
            StatusOptions.success = function (StatusList) {
                $("#statusID").val(StatusList.statusID);
            };
            StatusOptions.error = function (e) {
                alert("Error in Getting Status Details!!");
            };
            $.ajax(StatusOptions);
        }
        else {
            $("#statusID").val(0);
        }
    });
});

$(document).ready(function () {
    $("#lnkBlockUnblock").click(function () {

        var checkValues = $('input[name=chkBlockUnblock]:checked').map(function () {
            return $(this).val();
        }).get();
        if (checkValues.toString() == '') {
            return false;
        }
        var url = $("#lnkBlockUnblock").attr("href");
        var page = GetQueryStringParams("page");
        if (page == undefined)
            page = 1;
        $("#lnkBlockUnblock").attr("href", url + '?devices=' + checkValues.toString() + '&page=' + page);
    });

    function GetQueryStringParams(sParam) {
        var sURLVariables = window.location.search.substring(1).split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0] == sParam) {
                return sParameterName[1];
            }
        }
    }
});

