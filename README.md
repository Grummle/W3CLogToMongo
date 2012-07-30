W3CLogToMongo
=============

W3C Extended Log to Mongo

This is a utility to read W3C Extended Log formated log files. 
I use it for IIS 7.5 log files, though I believe Apache can output in W3C as well.

{
    "enabled": <bool>,                              #should this source be processed
    "batchSize": <int>,                             #
    "id": <string>,                                 #unique name of the source
    "logDirectory": <string>,                       #directory where the log files are     
    "entryLimit": <int>,                            #limit how many entries per file are process (buggy)
    "destination": {                                #Connection string info for mongo
        "mongoConnectionString": <string>,
        "mongoDatabase": <string>,
        "mongoCollection": <string>,
        "batchSize": <int>                          #How big the batch to send to mongo
    },
    "conversions": [                                #Change elements with this name to this type
        {
            "elementName": <string>,
            "type": <type>
        }
    ],
    "composites": [                                 #Create a new elemnet with these, joined by the 'glue'
        {
            "elementName": <string>,
            "elements": [
                <string>
            ],
            "glue": <string>
        }
    ],
    "aliases": [                                    #Rename Elements,applied before composite and conversions
        {
            "elementName":<string>,
            "alias":<string>
        }
    ],                                  
    "drop": [                                       #Do not include these elements when sending to mongo
        <string>
    ],
    "multiColumnElements": [                        #Designate which fields spna multiple columns
        {                                           #Added for MS IIS Advanced Logging
            "elementName":<string>,                 #BeginResponse-UTC,EndResponse-UTC span 2 columns (lame)
            "colSpan":<int>
        }
    ],
    "staticElements": [                             #Designate elements that should be added to all entries
        {
            "elementName": <string>,
            "elementValue": <string>
        }
    ],
    "logAll": <bool>,                              #Fist pick up gets all files.
    "templates": [                                 #Full path to json formated files that contain a valid source object.
        <string>
    ]
}