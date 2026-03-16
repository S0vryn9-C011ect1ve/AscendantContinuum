"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseLinkHeader = void 0;
function parseLinkHeader(header) {
    var links = {};
    if (header.length === 0) {
        throw new Error('input must not be of zero length');
    }
    var parts = header.split(',');
    parts.forEach(function (part) {
        var section = part.split(';');
        if (section.length !== 2) {
            throw new Error("section could not be split on ';'");
        }
        var url = section[0].replace(/<(.*)>/, '$1').trim();
        var name = section[1].replace(/rel="(.*)"/, '$1').trim();
        links[name] = url;
    });
    return links;
}
exports.parseLinkHeader = parseLinkHeader;
