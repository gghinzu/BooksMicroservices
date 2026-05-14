// ===== CONFIG =====
const BOOKS_API = 'http://localhost:5127/api';
const USERS_API = 'http://localhost:5055/api';

let token = null;
let currentUser = null;
let cachedAuthors = [];
let cachedGenres = [];
let cachedGroups = [];
let cachedRoles = [];

// ===== HELPERS =====
function $(id) { return document.getElementById(id); }
function show(el) { if (typeof el === 'string') el = $(el); if (el) el.style.display = ''; }
function hide(el) { if (typeof el === 'string') el = $(el); if (el) el.style.display = 'none'; }

function headers(auth = false) {
    const h = { 'Content-Type': 'application/json' };
    if (auth && token) h['Authorization'] = 'Bearer ' + token;
    return h;
}

async function api(base, path, method = 'GET', body = null, auth = false) {
    const opts = { method, headers: headers(auth) };
    if (body) opts.body = JSON.stringify(body);
    const res = await fetch(base + path, opts);
    if (res.status === 204) return [];
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || `HTTP ${res.status}`);
    }
    return res.json();
}

// ===== TOAST =====
function toast(msg, type = 'info') {
    const icons = { success: 'check_circle', error: 'error', info: 'info' };
    const t = document.createElement('div');
    t.className = `toast ${type}`;
    t.innerHTML = `<span class="material-symbols-rounded">${icons[type]}</span>
        <span class="toast-message">${msg}</span>
        <button class="toast-dismiss" onclick="this.parentElement.remove()">
            <span class="material-symbols-rounded">close</span></button>`;
    $('toast-container').appendChild(t);
    setTimeout(() => { t.classList.add('removing'); setTimeout(() => t.remove(), 300); }, 4000);
}

// ===== CONFIRM =====
function confirm(title, msg) {
    return new Promise(resolve => {
        $('confirm-title').textContent = title;
        $('confirm-message').textContent = msg;
        show('confirm-overlay');
        $('confirm-ok').onclick = () => { hide('confirm-overlay'); resolve(true); };
        $('confirm-cancel').onclick = () => { hide('confirm-overlay'); resolve(false); };
    });
}

// ===== MODAL =====
function openModal(title, html, onSave) {
    $('modal-title').textContent = title;
    $('modal-body').innerHTML = html;
    $('modal-overlay').classList.add('active');
    $('modal-save').onclick = async () => {
        try { await onSave(); closeModal(); } catch (e) { toast(e.message, 'error'); }
    };
}
function closeModal() { $('modal-overlay').classList.remove('active'); }
$('modal-close').addEventListener('click', closeModal);
$('modal-cancel').addEventListener('click', closeModal);
$('modal-overlay').addEventListener('click', e => { if (e.target === $('modal-overlay')) closeModal(); });

// ===== HEALTH CHECK =====
async function checkHealth() {
    try { await fetch(BOOKS_API + '/Books', { method: 'GET' }); $('books-api-status').className = 'status-dot online'; }
    catch { $('books-api-status').className = 'status-dot offline'; }
    try { await fetch(USERS_API + '/Users', { method: 'GET' }); $('users-api-status').className = 'status-dot online'; }
    catch { $('users-api-status').className = 'status-dot offline'; }
}

// ===== AUTH =====
$('login-form').addEventListener('submit', async e => {
    e.preventDefault();
    hide('login-error');
    const btn = $('btn-login');
    btn.querySelector('.btn-text').textContent = 'Signing in...';
    show(btn.querySelector('.btn-loader'));
    try {
        const data = await api(USERS_API, '/Token', 'POST', {
            userName: $('login-username').value,
            password: $('login-password').value
        });
        token = data.token;
        currentUser = $('login-username').value;
        $('user-name').textContent = currentUser;
        $('user-role').textContent = 'Authenticated';
        enterApp();
        toast('Signed in successfully!', 'success');
    } catch (err) {
        $('login-error').textContent = err.message || 'Invalid credentials';
        show('login-error');
    } finally {
        btn.querySelector('.btn-text').textContent = 'Sign In';
        hide(btn.querySelector('.btn-loader'));
    }
});

$('btn-skip').addEventListener('click', () => enterApp());

function enterApp() {
    hide('login-overlay');
    show('app');
    updateAdminUI();
    loadSection('books');
    checkHealth();
    setInterval(checkHealth, 30000);
}

function updateAdminUI() {
    const btns = ['btn-add-book','btn-add-author','btn-add-genre','btn-add-user','btn-add-role','btn-add-group'];
    btns.forEach(id => { if (token) show(id); else hide(id); });
}

$('btn-logout').addEventListener('click', () => {
    token = null; currentUser = null;
    $('user-name').textContent = 'Guest';
    $('user-role').textContent = 'No token';
    updateAdminUI();
    show('login-overlay');
    hide('app');
    toast('Signed out', 'info');
});

// ===== NAVIGATION =====
document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', e => {
        e.preventDefault();
        const section = item.dataset.section;
        document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
        item.classList.add('active');
        document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
        $('section-' + section).classList.add('active');
        $('page-title').textContent = section.charAt(0).toUpperCase() + section.slice(1);
        loadSection(section);
        // close mobile sidebar
        $('sidebar').classList.remove('open');
    });
});

$('sidebar-toggle').addEventListener('click', () => $('sidebar').classList.toggle('collapsed'));
$('mobile-menu').addEventListener('click', () => $('sidebar').classList.toggle('open'));

// ===== DATA LOADING =====
async function loadSection(section) {
    const loaders = { books: loadBooks, authors: loadAuthors, genres: loadGenres,
        users: loadUsers, roles: loadRoles, groups: loadGroups };
    if (loaders[section]) await loaders[section]();
}

// ----- BOOKS -----
async function loadBooks() {
    show('books-loading'); hide('books-empty');
    $('books-tbody').innerHTML = '';
    try {
        const books = await api(BOOKS_API, '/Books');
        hide('books-loading');
        $('books-count').textContent = books.length;
        if (!books.length) { show('books-empty'); return; }
        $('books-tbody').innerHTML = books.map(b => `<tr>
            <td>${b.id}</td>
            <td style="color:var(--text-primary);font-weight:500">${esc(b.name)}</td>
            <td>${esc(b.authorF || '')}</td>
            <td>${(b.genres||[]).map(g=>`<span class="tag tag-genre">${esc(g.name)}</span>`).join('')}</td>
            <td>${b.numberOfPages||'—'}</td>
            <td>${b.priceF||''}</td>
            <td>${b.publishDateF||''}</td>
            <td>${b.isTopSeller?'<span class="tag tag-top">★ Top Seller</span>':''}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editBook(${b.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteBook(${b.id},'${esc(b.name)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('books-loading'); show('books-empty'); toast('Failed to load books: '+e.message,'error'); }
}

async function editBook(id) {
    await ensureCaches();
    const books = await api(BOOKS_API, '/Books');
    const b = books.find(x=>x.id===id);
    if(!b) return;
    openModal('Edit Book', bookForm(b), async()=>{
        const body = getBookFormData(); body.id = id;
        await api(BOOKS_API, '/Books', 'PUT', body, true);
        toast('Book updated!','success'); loadBooks();
    });
}

async function deleteBook(id, name) {
    if(await confirm('Delete Book', `Delete "${name}"?`)){
        try{ await api(BOOKS_API,`/Books/${id}`,'DELETE',null,true); toast('Book deleted','success'); loadBooks(); }
        catch(e){ toast(e.message,'error'); }
    }
}

$('btn-add-book').addEventListener('click', async()=>{
    await ensureCaches();
    openModal('Add Book', bookForm(), async()=>{
        await api(BOOKS_API, '/Books', 'POST', getBookFormData(), true);
        toast('Book created!','success'); loadBooks();
    });
});

function bookForm(b={}){
    const authOpts = cachedAuthors.map(a=>`<option value="${a.id}" ${b.authorId===a.id?'selected':''}>${esc(a.firstName)} ${esc(a.lastName)}</option>`).join('');
    const genreChecks = cachedGenres.map(g=>`<label class="checkbox-group"><input type="checkbox" value="${g.id}" ${(b.genreIds||[]).includes(g.id)?'checked':''}>${esc(g.name)}</label>`).join('');
    return `
        <div class="form-group"><label>Name</label><input id="f-name" value="${esc(b.name||'')}" maxlength="50" required></div>
        <div class="form-row">
            <div class="form-group"><label>Author</label><select id="f-author"><option value="">Select</option>${authOpts}</select></div>
            <div class="form-group"><label>Pages</label><input id="f-pages" type="number" value="${b.numberOfPages||''}"></div>
        </div>
        <div class="form-row">
            <div class="form-group"><label>Price</label><input id="f-price" type="number" step="0.01" value="${b.price||0}"></div>
            <div class="form-group"><label>Publish Date</label><input id="f-pubdate" type="datetime-local" value="${b.publishDate?b.publishDate.slice(0,16):''}"></div>
        </div>
        <div class="form-group"><label class="checkbox-group"><input type="checkbox" id="f-top" ${b.isTopSeller?'checked':''}>Top Seller</label></div>
        <div class="form-group"><label>Genres</label><div id="f-genres" style="display:flex;flex-wrap:wrap;gap:8px">${genreChecks}</div></div>`;
}

function getBookFormData(){
    const genreEls = document.querySelectorAll('#f-genres input[type=checkbox]:checked');
    return {
        name: $('f-name').value,
        authorId: parseInt($('f-author').value)||0,
        numberOfPages: parseInt($('f-pages').value)||null,
        price: parseFloat($('f-price').value)||0,
        publishDate: $('f-pubdate').value||new Date().toISOString(),
        isTopSeller: $('f-top').checked,
        genreIds: Array.from(genreEls).map(e=>parseInt(e.value))
    };
}

// ----- AUTHORS -----
async function loadAuthors() {
    show('authors-loading'); hide('authors-empty');
    $('authors-tbody').innerHTML = '';
    try {
        const data = await api(BOOKS_API, '/Authors');
        cachedAuthors = data;
        hide('authors-loading');
        $('authors-count').textContent = data.length;
        if (!data.length) { show('authors-empty'); return; }
        $('authors-tbody').innerHTML = data.map(a => `<tr>
            <td>${a.id}</td>
            <td style="color:var(--text-primary)">${esc(a.firstName)}</td>
            <td style="color:var(--text-primary)">${esc(a.lastName)}</td>
            <td>${a.books?a.books.length:0}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editAuthor(${a.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteAuthor(${a.id},'${esc(a.firstName)} ${esc(a.lastName)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('authors-loading'); show('authors-empty'); toast('Failed: '+e.message,'error'); }
}

$('btn-add-author').addEventListener('click', ()=>{
    openModal('Add Author', authorForm(), async()=>{
        await api(BOOKS_API,'/Authors','POST',{firstName:$('f-fname').value,lastName:$('f-lname').value},true);
        toast('Author created!','success'); loadAuthors();
    });
});

async function editAuthor(id){
    const a = cachedAuthors.find(x=>x.id===id);
    if(!a)return;
    openModal('Edit Author', authorForm(a), async()=>{
        await api(BOOKS_API,'/Authors','PUT',{id,firstName:$('f-fname').value,lastName:$('f-lname').value},true);
        toast('Author updated!','success'); loadAuthors();
    });
}

async function deleteAuthor(id,name){
    if(await confirm('Delete Author',`Delete "${name}"?`)){
        try{ await api(BOOKS_API,`/Authors/${id}`,'DELETE',null,true); toast('Deleted','success'); loadAuthors(); }
        catch(e){ toast(e.message,'error'); }
    }
}

function authorForm(a={}){
    return `<div class="form-row">
        <div class="form-group"><label>First Name</label><input id="f-fname" value="${esc(a.firstName||'')}" maxlength="50"></div>
        <div class="form-group"><label>Last Name</label><input id="f-lname" value="${esc(a.lastName||'')}" maxlength="50"></div>
    </div>`;
}

// ----- GENRES -----
async function loadGenres() {
    show('genres-loading'); hide('genres-empty');
    $('genres-tbody').innerHTML = '';
    try {
        const data = await api(BOOKS_API, '/Genres');
        cachedGenres = data;
        hide('genres-loading');
        $('genres-count').textContent = data.length;
        if (!data.length) { show('genres-empty'); return; }
        $('genres-tbody').innerHTML = data.map(g => `<tr>
            <td>${g.id}</td>
            <td style="color:var(--text-primary)">${esc(g.name)}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editGenre(${g.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteGenre(${g.id},'${esc(g.name)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('genres-loading'); show('genres-empty'); toast('Failed: '+e.message,'error'); }
}

$('btn-add-genre').addEventListener('click', ()=>{
    openModal('Add Genre', `<div class="form-group"><label>Name</label><input id="f-gname" maxlength="50"></div>`, async()=>{
        await api(BOOKS_API,'/Genres','POST',{name:$('f-gname').value},true);
        toast('Genre created!','success'); loadGenres();
    });
});

async function editGenre(id){
    const g = cachedGenres.find(x=>x.id===id);
    openModal('Edit Genre', `<div class="form-group"><label>Name</label><input id="f-gname" value="${esc(g.name)}" maxlength="50"></div>`, async()=>{
        await api(BOOKS_API,'/Genres','PUT',{id,name:$('f-gname').value},true);
        toast('Genre updated!','success'); loadGenres();
    });
}

async function deleteGenre(id,name){
    if(await confirm('Delete Genre',`Delete "${name}"?`)){
        try{ await api(BOOKS_API,`/Genres/${id}`,'DELETE',null,true); toast('Deleted','success'); loadGenres(); }
        catch(e){ toast(e.message,'error'); }
    }
}

// ----- USERS -----
async function loadUsers() {
    show('users-loading'); hide('users-empty');
    $('users-tbody').innerHTML = '';
    try {
        const data = await api(USERS_API, '/Users');
        hide('users-loading');
        $('users-count').textContent = data.length;
        if (!data.length) { show('users-empty'); return; }
        $('users-tbody').innerHTML = data.map(u => `<tr>
            <td>${u.id}</td>
            <td style="color:var(--text-primary);font-weight:500">${esc(u.userName)}</td>
            <td>${esc(u.fullName||'')}</td>
            <td>${u.genderF||''}</td>
            <td>${esc(u.groupF||'—')}</td>
            <td>${(u.rolesF||[]).map(r=>`<span class="tag tag-role">${esc(r)}</span>`).join('')||'—'}</td>
            <td>${u.scoreF||'0'}</td>
            <td><span class="tag ${u.isActive?'tag-active':'tag-inactive'}">${u.isActiveF}</span></td>
            <td>${u.registrationDateF||''}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editUser(${u.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteUser(${u.id},'${esc(u.userName)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('users-loading'); show('users-empty'); toast('Failed: '+e.message,'error'); }
}

$('btn-add-user').addEventListener('click', async()=>{
    await ensureUserCaches();
    openModal('Add User', userForm(), async()=>{
        await api(USERS_API,'/Users','POST',getUserFormData(),true);
        toast('User created!','success'); loadUsers();
    });
});

async function editUser(id){
    await ensureUserCaches();
    const users = await api(USERS_API,'/Users');
    const u = users.find(x=>x.id===id);
    if(!u)return;
    openModal('Edit User', userForm(u), async()=>{
        const body = getUserFormData(); body.id = id;
        await api(USERS_API,'/Users','PUT',body,true);
        toast('User updated!','success'); loadUsers();
    });
}

async function deleteUser(id,name){
    if(await confirm('Delete User',`Delete "${name}"?`)){
        try{ await api(USERS_API,`/Users/${id}`,'DELETE',null,true); toast('Deleted','success'); loadUsers(); }
        catch(e){ toast(e.message,'error'); }
    }
}

function userForm(u={}){
    const grpOpts = cachedGroups.map(g=>`<option value="${g.id}" ${u.groupId===g.id?'selected':''}>${esc(g.title)}</option>`).join('');
    const roleChecks = cachedRoles.map(r=>`<label class="checkbox-group"><input type="checkbox" value="${r.id}" ${(u.roleIds||[]).includes(r.id)?'checked':''}>${esc(r.name)}</label>`).join('');
    const bd = u.birthDate?u.birthDate.slice(0,10):'';
    return `
        <div class="form-row">
            <div class="form-group"><label>Username</label><input id="f-uname" value="${esc(u.userName||'')}" maxlength="30" required></div>
            <div class="form-group"><label>Password</label><input id="f-upass" type="password" value="${esc(u.password||'')}" maxlength="15" required></div>
        </div>
        <div class="form-row">
            <div class="form-group"><label>First Name</label><input id="f-ufname" value="${esc(u.firstName||'')}" maxlength="50"></div>
            <div class="form-group"><label>Last Name</label><input id="f-ulname" value="${esc(u.lastName||'')}" maxlength="50"></div>
        </div>
        <div class="form-row">
            <div class="form-group"><label>Gender</label><select id="f-ugender"><option value="1" ${u.gender===1?'selected':''}>Woman</option><option value="2" ${u.gender===2?'selected':''}>Man</option></select></div>
            <div class="form-group"><label>Birth Date</label><input id="f-ubirth" type="date" value="${bd}"></div>
        </div>
        <div class="form-row">
            <div class="form-group"><label>Score</label><input id="f-uscore" type="number" step="0.1" value="${u.score||0}"></div>
            <div class="form-group"><label>Group</label><select id="f-ugroup"><option value="">None</option>${grpOpts}</select></div>
        </div>
        <div class="form-group"><label>Address</label><input id="f-uaddr" value="${esc(u.address||'')}"></div>
        <div class="form-group"><label class="checkbox-group"><input type="checkbox" id="f-uactive" ${u.isActive?'checked':''}>Active</label></div>
        <div class="form-group"><label>Roles</label><div id="f-uroles" style="display:flex;flex-wrap:wrap;gap:8px">${roleChecks}</div></div>`;
}

function getUserFormData(){
    const roleEls = document.querySelectorAll('#f-uroles input[type=checkbox]:checked');
    return {
        userName: $('f-uname').value,
        password: $('f-upass').value,
        firstName: $('f-ufname').value,
        lastName: $('f-ulname').value,
        gender: parseInt($('f-ugender').value),
        birthDate: $('f-ubirth').value||null,
        registrationDate: new Date().toISOString(),
        score: parseFloat($('f-uscore').value)||0,
        isActive: $('f-uactive').checked,
        address: $('f-uaddr').value,
        groupId: parseInt($('f-ugroup').value)||null,
        roleIds: Array.from(roleEls).map(e=>parseInt(e.value))
    };
}

// ----- ROLES -----
async function loadRoles() {
    show('roles-loading'); hide('roles-empty');
    $('roles-tbody').innerHTML = '';
    try {
        const data = await api(USERS_API, '/Roles');
        cachedRoles = data;
        hide('roles-loading');
        $('roles-count').textContent = data.length;
        if (!data.length) { show('roles-empty'); return; }
        $('roles-tbody').innerHTML = data.map(r => `<tr>
            <td>${r.id}</td><td style="color:var(--text-primary)">${esc(r.name)}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editRole(${r.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteRole(${r.id},'${esc(r.name)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('roles-loading'); show('roles-empty'); toast('Failed: '+e.message,'error'); }
}

$('btn-add-role').addEventListener('click', ()=>{
    openModal('Add Role', `<div class="form-group"><label>Name</label><input id="f-rname" maxlength="25"></div>`, async()=>{
        await api(USERS_API,'/Roles','POST',{name:$('f-rname').value},true);
        toast('Role created!','success'); loadRoles();
    });
});

async function editRole(id){
    const r = cachedRoles.find(x=>x.id===id);
    openModal('Edit Role', `<div class="form-group"><label>Name</label><input id="f-rname" value="${esc(r.name)}" maxlength="25"></div>`, async()=>{
        await api(USERS_API,'/Roles','PUT',{id,name:$('f-rname').value},true);
        toast('Role updated!','success'); loadRoles();
    });
}

async function deleteRole(id,name){
    if(await confirm('Delete Role',`Delete "${name}"?`)){
        try{ await api(USERS_API,`/Roles/${id}`,'DELETE',null,true); toast('Deleted','success'); loadRoles(); }
        catch(e){ toast(e.message,'error'); }
    }
}

// ----- GROUPS -----
async function loadGroups() {
    show('groups-loading'); hide('groups-empty');
    $('groups-tbody').innerHTML = '';
    try {
        const data = await api(USERS_API, '/Groups');
        cachedGroups = data;
        hide('groups-loading');
        $('groups-count').textContent = data.length;
        if (!data.length) { show('groups-empty'); return; }
        $('groups-tbody').innerHTML = data.map(g => `<tr>
            <td>${g.id}</td><td style="color:var(--text-primary)">${esc(g.title)}</td>
            <td class="actions-cell">${token?`
                <button class="btn-icon edit" onclick="editGroup(${g.id})"><span class="material-symbols-rounded">edit</span></button>
                <button class="btn-icon delete" onclick="deleteGroup(${g.id},'${esc(g.title)}')"><span class="material-symbols-rounded">delete</span></button>`:''}</td>
        </tr>`).join('');
    } catch(e) { hide('groups-loading'); show('groups-empty'); toast('Failed: '+e.message,'error'); }
}

$('btn-add-group').addEventListener('click', ()=>{
    openModal('Add Group', `<div class="form-group"><label>Title</label><input id="f-gtitle" maxlength="100"></div>`, async()=>{
        await api(USERS_API,'/Groups','POST',{title:$('f-gtitle').value},true);
        toast('Group created!','success'); loadGroups();
    });
});

async function editGroup(id){
    const g = cachedGroups.find(x=>x.id===id);
    openModal('Edit Group', `<div class="form-group"><label>Title</label><input id="f-gtitle" value="${esc(g.title)}" maxlength="100"></div>`, async()=>{
        await api(USERS_API,'/Groups','PUT',{id,title:$('f-gtitle').value},true);
        toast('Group updated!','success'); loadGroups();
    });
}

async function deleteGroup(id,name){
    if(await confirm('Delete Group',`Delete "${name}"?`)){
        try{ await api(USERS_API,`/Groups/${id}`,'DELETE',null,true); toast('Deleted','success'); loadGroups(); }
        catch(e){ toast(e.message,'error'); }
    }
}

// ===== CACHE HELPERS =====
async function ensureCaches(){
    try{ if(!cachedAuthors.length) cachedAuthors = await api(BOOKS_API,'/Authors'); }catch(e){}
    try{ if(!cachedGenres.length) cachedGenres = await api(BOOKS_API,'/Genres'); }catch(e){}
}
async function ensureUserCaches(){
    try{ if(!cachedGroups.length) cachedGroups = await api(USERS_API,'/Groups'); }catch(e){}
    try{ if(!cachedRoles.length) cachedRoles = await api(USERS_API,'/Roles'); }catch(e){}
}

function esc(s){ if(!s) return ''; const d=document.createElement('div'); d.textContent=s; return d.innerHTML; }
