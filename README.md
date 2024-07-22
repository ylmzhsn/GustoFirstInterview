# RandomInterview

Log analyzer

Your goal is to parse a log file and do some analysis on it. The log file contains all requests to a server within a specific timeframe.

We're interested in statistics about for the following method/URL definitions:

GET /api/users/{user_id}/count_pending_messages
GET /api/users/{user_id}/get_messages
GET /api/users/{user_id}/get_friends_progress
GET /api/users/{user_id}/get_friends_score
POST /api/users/{user_id}
GET /api/users/{user_id}

Where user_id is the user id of the users calling the backend.

The script/program should output a small analysis of the sample. It should contain the following information for each of the URLs above:

* The number of times the URL was called.
* Mean (average) response times (connect time + service time)
* Median response times (connect time + service time)

The output should be an array of dictionary/hashes, one per URL.

The log format is defined as:
{timestamp} {source}[{process}]: at={log_level} method={http_method} path={http_path} host={http_host} fwd={client_ip} dyno={responding_dyno} connect={connection_time}ms service={processing_time}ms status={http_status} bytes={bytes_sent}

Example:
2014-01-09T06:16:53.916977+00:00 heroku[router]: at=info method=GET path=/api/users/1538823671/count_pending_messages host=mygame.heroku.com fwd="208.54.86.162" dyno=web.11 connect=5ms service=10ms status=200 bytes=33
2014-01-09T06:18:53.014475+00:00 heroku[router]: at=info method=GET path=/api/users/78475839/count_pending_messages host=mygame.heroku.com fwd="208.54.86.162" dyno=web.10 connect=5ms service=20ms status=200 bytes=33
2014-01-09T06:20:33.142889+00:00 heroku[router]: at=info method=GET path=/api/users/847383/count_pending_messages host=mygame.heroku.com fwd="208.54.86.162" dyno=web.10 connect=25ms service=55ms status=200 bytes=33

Given the above three log lines, we would expect output like:

[{
   "request_identifier": "GET /api/users/{user_id}/count_pending_messages",
   "called": 3,
   "response_time_mean": 40.0,
   "response_time_median": 25.0,
}]
