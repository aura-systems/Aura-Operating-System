/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Route class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   David Jeske
*                   Barend Erasmus
* LICENSE:          LICENSES\SimpleHttpServer\LICENSE.md
*/

using System;

namespace SimpleHttpServer.Models
{
    public class Route
    {
        public string Name { get; set; } // descriptive name for debugging
        public string Method { get; set; }
        public Action<HttpDiscussion> Callable;
    }
}
