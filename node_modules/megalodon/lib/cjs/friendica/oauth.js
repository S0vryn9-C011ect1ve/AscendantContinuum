"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.toTokenData = exports.toAppData = void 0;
function toAppData(appData) {
    return {
        id: appData.id,
        name: appData.name,
        website: appData.website,
        redirect_uri: appData.redirect_uri,
        client_id: appData.client_id,
        client_secret: appData.client_secret,
        url: null,
        session_token: null
    };
}
exports.toAppData = toAppData;
function toTokenData(tokenData) {
    return {
        access_token: tokenData.access_token,
        token_type: tokenData.token_type,
        scope: tokenData.scope,
        created_at: tokenData.created_at,
        expires_in: null,
        refresh_token: null
    };
}
exports.toTokenData = toTokenData;
