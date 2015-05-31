// ParsleyConfig definition if not already set
window.ParsleyConfig = window.ParsleyConfig || {};
window.ParsleyConfig.i18n = window.ParsleyConfig.i18n || {};

// Define then the messages
window.ParsleyConfig.i18n.vi = $.extend(window.ParsleyConfig.i18n.vi || {}, {
  defaultMessage: "This value seems to be invalid.",
  type: {
    email:        "Email không hợp lệ",
    url:          "URL không hợp lệ",
    number:       "Dữ liệu phải ở dạng số",
    integer:      "Dữ liệu nhập phải có dạng số.",
    digits:       "This value should be digits.",
    alphanum:     "Dữ liệu phải ở dạng ký tự"
  },
  notblank:       "Dữ liệu không được để trống",
  required:       "Vui lòng nhập dữ liệu",
  pattern:        "Dữ liệu không hợp lệ.",
  min:            "Trường dữ liệu phải lớn hơn hoặc bằng %s",
  max:            "Trường dữ liệu phải nhỏ hơn hoặc bằng %s",
  range:          "Trường dữ liệu phải trong khoảng %s và %s",
  minlength:      "Trường dữ liệu phải có ít nhất %s ký tự",
  maxlength:      "Trường dữ liệu không được quá %s ký tự",
  length:         "Trường dữ liệu phải nằm trong khoảng %s và %s ký tự",
  mincheck:       "You must select at least %s choices.",
  maxcheck:       "You must select %s choices or less.",
  check:          "You must select between %s and %s choices.",
  equalto:        "Dữ liệu không trùng khớp."
});

// If file is loaded after Parsley main file, auto-load locale
if ('undefined' !== typeof window.ParsleyValidator)
  window.ParsleyValidator.addCatalog('vi', window.ParsleyConfig.i18n.vi, true);
