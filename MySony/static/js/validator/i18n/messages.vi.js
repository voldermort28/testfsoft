window.ParsleyConfig = window.ParsleyConfig || {};

(function ($) {
  window.ParsleyConfig = $.extend( true, {}, window.ParsleyConfig, {
    messages: {
      // parsley //////////////////////////////////////
        defaultMessage: "Dữ liệu không hợp lệ."
        , type: {
            email:      "Email không hợp lệ."
          , url:        "Đường dẫn không hợp lệ."
          , urlstrict:  "Đường dẫn không hợp lệ."
          , number:     "Dữ liệu phải là kiểu số."
          , digits:     "Dữ liệu phải là kiểu số."
          , dateIso:    "Dữ liệu không hợp lệ."
          , alphanum:   "Dữ liệu phải là kiểu số."
          , phone:      "Dữ liệu không hợp lệ."
        }
      , notnull:        "Vui lòng nhập thông tin."
      , notblank:       "Dữ liệu nhập vào không được có khoảng trắng."
      , required:       "Vui lòng nhập thông tin."
      , regexp:         "Dữ liệu không hợp lệ."
      , min:            "Dữ liệu nhập vào phải lớn hơn hoặc bằng %s."
      , max:            "Dữ liệu nhập vào phải nhỏ hơn hoặc bằng %s."
      , range:          "Dữ liệu nhập vào phải lớn hơn %s và nhỏ hơn %s."
      , minlength:      "Dữ liệu nhập vào phải chứa ít nhất %s ký tự."
      , maxlength:      "Dữ liệu nhập vào không được quá %s ký tự."
      , rangelength:    "Dữ liệu nhập vào chứa ít nhất %s ký tự và không quá %s ký tự."
      , mincheck:       "Vui lòng chọn ít nhất %s giá trị."
      , maxcheck:       "Vui lòng chọn không quá %s giá trị."
      , rangecheck:     "Vui lòng chọn ít nhất %s giá trị và không quá %s giá trị."
      , equalto:        "Dữ liệu không trùng khớp."

      // parsley.extend ///////////////////////////////
      , minwords:       "Dữ liệu nhập vào phải chứa ít nhất %s từ."
      , maxwords:       "Dữ liệu nhập vào không được quá %s từ."
      , rangewords:     "Dữ liệu nhập vào chứa ít nhất %s từ và không quá %s từ."
      , greaterthan:    "Dữ liệu nhập vào không được quá %s."
      , lessthan:       "Dữ liệu nhập vào không được ít hơn %s."
      , beforedate:     "Ngày nhập vào phải trước %s."
      , afterdate:      "Ngày nhập vào phải sau %s."
      , americandate:		"Ngày nhập vào không hợp lệ."
    }
  });
}(window.jQuery || window.Zepto));