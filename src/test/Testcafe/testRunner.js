const createTestCafe = require('testcafe');
const fs = require('fs');
let testcafe = null;

createTestCafe()
  .then(tc => {
    testcafe = tc;
    const runner = testcafe.createRunner();
    const stream = fs.createWriteStream('junit.xml');

    return runner
      .browsers(['chrome'])
      .concurrency(1)
      .screenshots('./screenshots', { takeOnFails: true })
      .reporter('junit', stream)
      .src(['./*_tests/*.js'])
      .run({
        skipJsErrors: true,
        quarantineMode: false,
        selectorTimeout: 10000,
        assertionTimeout: 20000,
        pageLoadTimeout: 10000,
        speed: 1,
        debugOnFail: false,
        stopOnFirstFail: false
      })
      .then(failedCount => {
        console.log('Total tests failed ' + failedCount);
        stream.end();
      })
  })
  .then(() => {
    testcafe.close();
  });
