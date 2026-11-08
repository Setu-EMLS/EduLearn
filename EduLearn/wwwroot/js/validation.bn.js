// Bangla messages for the browser-side form checks (jquery-validate). Loaded only on Bangla pages.
(function ($) {
    if (!$ || !$.validator) return;
    $.extend($.validator.messages, {
        required: 'এই তথ্যটি দেওয়া আবশ্যক।',
        remote: 'অনুগ্রহ করে এই তথ্যটি ঠিক করুন।',
        email: 'সঠিক ইমেইল ঠিকানা দিন।',
        url: 'সঠিক ওয়েব ঠিকানা (URL) দিন।',
        date: 'সঠিক তারিখ দিন।',
        dateISO: 'সঠিক তারিখ দিন (বছর-মাস-দিন)।',
        number: 'সঠিক সংখ্যা দিন।',
        digits: 'শুধু সংখ্যা দিন।',
        creditcard: 'সঠিক কার্ড নম্বর দিন।',
        equalTo: 'একই মান আবার লিখুন।',
        maxlength: $.validator.format('সর্বোচ্চ {0}টি অক্ষর দেওয়া যাবে।'),
        minlength: $.validator.format('কমপক্ষে {0}টি অক্ষর দিন।'),
        rangelength: $.validator.format('{0} থেকে {1}টি অক্ষরের মধ্যে লিখুন।'),
        range: $.validator.format('{0} থেকে {1}-এর মধ্যে একটি মান দিন।'),
        max: $.validator.format('{0}-এর বেশি হতে পারবে না।'),
        min: $.validator.format('{0}-এর কম হতে পারবে না।')
    });
})(window.jQuery);
