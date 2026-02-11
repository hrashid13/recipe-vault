// Newsletter Composer JavaScript
var quill;

function initNewsletterComposer() {
    // Initialize Quill editor
    quill = new Quill('#editor', {
        theme: 'snow',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'align': [] }],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                ['link'],
                ['clean']
            ]
        },
        placeholder: 'Start writing your newsletter content here...'
    });

    // Form submit handler
    var form = document.getElementById('newsletterForm');
    if (form) {
        form.onsubmit = function() {
            var textarea = document.getElementById('htmlContent');
            if (textarea && quill) {
                textarea.value = quill.root.innerHTML;
                var count = document.querySelector('[data-subscriber-count]');
                var subscriberCount = count ? count.getAttribute('data-subscriber-count') : '0';
                return confirm('Are you sure you want to send this newsletter to ' + subscriberCount + ' subscribers?');
            }
            return false;
        };
    }

    // Template buttons
    var templateButtons = document.querySelectorAll('.template-btn');
    for (var i = 0; i < templateButtons.length; i++) {
        templateButtons[i].addEventListener('click', function() {
            var templateType = this.getAttribute('data-template');
            loadTemplate(templateType);
        });
    }

    // Recipe items
    var recipeItems = document.querySelectorAll('.recipe-item');
    for (var i = 0; i < recipeItems.length; i++) {
        recipeItems[i].addEventListener('click', function() {
            var id = this.getAttribute('data-id');
            var name = this.getAttribute('data-name');
            var desc = this.getAttribute('data-desc');
            var cuisine = this.getAttribute('data-cuisine');
            var prepTime = this.getAttribute('data-prep');
            
            // Parse JSON if needed
            try {
                name = JSON.parse(name);
                desc = JSON.parse(desc);
            } catch(e) {
                // Already plain text
            }
            
            insertRecipe(id, name, desc, cuisine, prepTime);
        });
    }

    // Preview button
    var previewBtn = document.getElementById('previewBtn');
    if (previewBtn) {
        previewBtn.addEventListener('click', function() {
            previewNewsletter();
        });
    }
}

function loadTemplate(templateType) {
    fetch('/admin/newsletter/get-template', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ templateType: templateType })
    })
    .then(function(response) { 
        return response.json(); 
    })
    .then(function(data) {
        if (quill && data.html) {
            quill.clipboard.dangerouslyPasteHTML(data.html);
        }
    })
    .catch(function(error) {
        console.error('Error loading template:', error);
        alert('Error loading template. Please try again.');
    });
}

function insertRecipe(id, name, desc, cuisine, prepTime) {
    if (!quill) {
        console.error('Quill editor not initialized');
        return;
    }
    
    var html = '';
    html += '<div style="margin: 20px 0; padding: 20px; border: 1px solid #ddd; border-radius: 8px;">';
    html += '<h3 style="margin-top: 0; color: #667eea;">';
    html += name;
    html += '</h3>';
    html += '<p>';
    html += desc;
    html += '</p>';
    html += '<p style="color: #666; font-size: 14px;">';
    html += 'Prep Time: ' + prepTime + ' min | Cuisine: ' + cuisine;
    html += '</p>';
    html += '<a href="https://recipesvault.org/recipes/details/' + id + '" ';
    html += 'style="display: inline-block; padding: 10px 20px; background: #667eea; color: white; text-decoration: none; border-radius: 5px;">';
    html += 'View Recipe';
    html += '</a>';
    html += '</div>';
    
    var position = quill.getLength();
    quill.clipboard.dangerouslyPasteHTML(position, html);
}

function previewNewsletter() {
    if (!quill) {
        console.error('Quill editor not initialized');
        return;
    }
    
    var content = quill.root.innerHTML;
    var previewWindow = window.open('', 'Newsletter Preview', 'width=800,height=600');
    
    var htmlDoc = '<!DOCTYPE html>';
    htmlDoc += '<html>';
    htmlDoc += '<head>';
    htmlDoc += '<title>Newsletter Preview</title>';
    htmlDoc += '<style>';
    htmlDoc += 'body { font-family: Arial, sans-serif; padding: 20px; background: #f4f4f4; }';
    htmlDoc += '.preview-container { max-width: 600px; margin: 0 auto; background: white; padding: 20px; }';
    htmlDoc += '</style>';
    htmlDoc += '</head>';
    htmlDoc += '<body>';
    htmlDoc += '<div class="preview-container">';
    htmlDoc += content;
    htmlDoc += '</div>';
    htmlDoc += '</body>';
    htmlDoc += '</html>';
    
    previewWindow.document.write(htmlDoc);
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initNewsletterComposer);
} else {
    initNewsletterComposer();
}
