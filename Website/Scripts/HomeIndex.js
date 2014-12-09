document.addEventListener('DOMContentLoaded', function() {
    var inputTextArea = document.querySelector('textarea');
    var submitButton = document.querySelector('button');
    submitButton.disabled = true;

    submitButton.addEventListener('click',function(e) {
        if (this.disabled) {
            console.log('no cigar kiddo!');
            return;
        }

        var request = new XMLHttpRequest();
        request.open('POST', '/home/uploadjson');

        request.onload = function() {
            if (this.status === 200) {
                console.dir(this.response);
            }
        }

        request.send(inputTextArea.value);
    });
    
    function validateInput() {

        try {
            JSON.parse(inputTextArea.value);
            inputTextArea.className = 'valid';
            submitButton.disabled = false;
        } catch (e) {
            inputTextArea.className = 'invalid';
            submitButton.disabled = true;
        }
    };

    inputTextArea.addEventListener('keyup', function(e) {
        console.log(inputTextArea.value);
    });

    inputTextArea.addEventListener('dragover', function(e) {
        this.className = 'hover';
        return false;
    });

    inputTextArea.addEventListener('dragend', function(e) {
        this.className = '';
        return false;
    });

    inputTextArea.addEventListener('drop', function(e) {
        this.className = '';
        e.preventDefault();

        var file = e.dataTransfer.files[0];
        var reader = new FileReader();
        reader.onload = function() {
            inputTextArea.value = reader.result;
            validateInput();
        }

        reader.readAsText(file, 'UTF-8');
       
        return false;
    });
})