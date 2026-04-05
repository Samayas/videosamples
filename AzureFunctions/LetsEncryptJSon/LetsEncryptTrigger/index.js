const fs = require('fs');
const path = require('path');

module.exports = async function (context, req) {
    const token = context.bindingData.token;
    context.log(`ACME challenge requested for token: ${token}`);

    // Block any path traversal attempts early - ACME tokens are only alphanumeric + hyphens
    const safeTokenPattern = /^[a-zA-Z0-9_\-]+$/;
    if (!token || !safeTokenPattern.test(token)) {
        context.log(`Rejected invalid token: ${token}`);
        context.res = {
            status: 400,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Invalid token format.'
        };
        return;
    }

    const baseDir = path.resolve('D:\\home\\site\\wwwroot\\.well-known\\acme-challenge');
    const filePath = path.resolve(baseDir, token);

    // Ensure the resolved path is strictly inside the base directory
    if (!filePath.startsWith(baseDir + path.sep) && filePath !== baseDir) {
        context.log(`Path traversal attempt detected: ${filePath}`);
        context.res = {
            status: 403,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Access denied.'
        };
        return;
    }

    if (!fs.existsSync(filePath)) {
        context.log(`Challenge file not found: ${filePath}`);
        context.res = {
            status: 404,
            headers: { 'Content-Type': 'text/plain' },
            body: 'ACME challenge token not found: ' + token
        };
        return;
    }

    const content = fs.readFileSync(filePath, 'utf8');
    context.log(`Returning challenge content for token: ${token}`);

    context.res = {
        status: 200,
        headers: { 'Content-Type': 'text/plain' },
        body: content
    };
};