using System;
using System.Text;

namespace Aura_OS.System.Network.HTTP.Client
{
    public class BasicUri
    {
        public string UserInfo { get; private set; }
        public string Scheme { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Path { get; private set; }
        public string Query { get; private set; }
        public string Fragment { get; private set; }

        public string Authority
        {
            get
            {
                string userInfoPart = string.IsNullOrEmpty(UserInfo) ? "" : $"{UserInfo}@";
                string portPart = (Scheme.ToLower() == "http" && Port == 80) ||
                                  (Scheme.ToLower() == "https" && Port == 443)
                                      ? ""
                                      : $":{Port}";
                return $"{userInfoPart}{Host}{portPart}";
            }
        }

        public string PathAndQuery
        {
            get
            {
                return $"{Path}{(string.IsNullOrEmpty(Query) ? "" : $"?{Query}")}";
            }
        }

        public string AbsoluteUri
        {
            get
            {
                var uriBuilder = new StringBuilder();
                uriBuilder.Append($"{Scheme}://");
                uriBuilder.Append(Authority);
                uriBuilder.Append(Path);
                if (!string.IsNullOrEmpty(Query))
                {
                    uriBuilder.Append($"?{Query}");
                }
                if (!string.IsNullOrEmpty(Fragment))
                {
                    uriBuilder.Append($"#{Fragment}");
                }
                return uriBuilder.ToString();
            }
        }

        public string AbsolutePath
        {
            get
            {
                // Ici, nous retournons simplement le chemin.
                // Dans un vrai cas d'utilisation, vous pourriez vouloir vous assurer que le chemin commence par "/"
                // et effectuer un nettoyage ou une normalisation supplémentaire.
                return Path;
            }
        }

        public static bool IsWellFormedUriString(string uriString, UriKind uriKind)
        {
            // Tentative simple de parsing sans regex.
            try
            {
                if (string.IsNullOrEmpty(uriString))
                {
                    return false;
                }

                // On utilise la logique de notre parseur de BasicUri.
                var tempUri = new BasicUri(uriString);

                switch (uriKind)
                {
                    case UriKind.Absolute:
                        return !string.IsNullOrEmpty(tempUri.Scheme) &&
                               !string.IsNullOrEmpty(tempUri.Host);
                    case UriKind.Relative:
                        // Pour un URI relatif, le schéma et l'autorité doivent être absents.
                        return string.IsNullOrEmpty(tempUri.Scheme) &&
                               string.IsNullOrEmpty(tempUri.Host) &&
                               !string.IsNullOrEmpty(tempUri.Path);
                    case UriKind.RelativeOrAbsolute:
                        // Pour un URI relatif ou absolu, nous acceptons les deux cas.
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(uriKind));
                }
            }
            catch
            {
                // Si une exception a été levée pendant l'analyse, l'URI n'est pas bien formé.
                return false;
            }
        }

        public BasicUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            ParseUri(uri);
        }

        private void ParseUri(string uri)
        {
            // Validation initiale
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            // Trouver la fin du schéma
            int schemeEnd = uri.IndexOf("://", StringComparison.Ordinal);
            if (schemeEnd == -1)
                throw new FormatException("Invalid URI: The URI should have a scheme.");

            Scheme = uri.Substring(0, schemeEnd);

            // Trouver le début et la fin de l'autorité
            int authorityStart = schemeEnd + 3; // passer "://"
            int authorityEnd = uri.IndexOfAny(new[] { '/', '?', '#' }, authorityStart);
            if (authorityEnd == -1) authorityEnd = uri.Length;

            // Extrait l'autorité et la divise en UserInfo, Host et Port
            string authority = uri.Substring(authorityStart, authorityEnd - authorityStart);
            ParseAuthority(authority);

            // Extrait le Path
            int pathStart = authorityEnd;
            int pathEnd = uri.IndexOfAny(new[] { '?', '#' }, pathStart);
            if (pathEnd == -1) pathEnd = uri.Length;

            Path = uri.Substring(pathStart, pathEnd - pathStart);

            // Extrait la Query
            if (pathEnd < uri.Length && uri[pathEnd] == '?')
            {
                int queryStart = pathEnd + 1;
                int queryEnd = uri.IndexOf('#', queryStart);
                if (queryEnd == -1) queryEnd = uri.Length;

                Query = uri.Substring(queryStart, queryEnd - queryStart);
                pathEnd = queryEnd;
            }

            // Extrait le Fragment
            if (pathEnd < uri.Length && uri[pathEnd] == '#')
            {
                int fragmentStart = pathEnd + 1;
                Fragment = uri.Substring(fragmentStart);
            }
        }

        private void ParseAuthority(string authority)
        {
            int userInfoEnd = authority.IndexOf('@');
            if (userInfoEnd != -1)
            {
                UserInfo = authority.Substring(0, userInfoEnd);
                authority = authority.Substring(userInfoEnd + 1);
            }

            int portStart = authority.LastIndexOf(':');
            if (portStart != -1)
            {
                string portStr = authority.Substring(portStart + 1);
                if (!int.TryParse(portStr, out var port) || port <= 0 || port > 65535)
                    throw new FormatException("Invalid URI: Port is not valid.");

                Port = port;
                Host = authority.Substring(0, portStart);
            }
            else
            {
                Host = authority;
                Port = Scheme.ToLower() == "https" ? 443 : 80; // port par défaut
            }
        }

        public override string ToString()
        {
            return $"{Scheme}://{Host}:{Port}{Path}{(string.IsNullOrEmpty(Query) ? "" : "?" + Query)}{(string.IsNullOrEmpty(Fragment) ? "" : "#" + Fragment)}";
        }
    }
}
