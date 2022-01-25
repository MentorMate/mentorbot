module.exports = {
  moduleNameMapper: {
    '@core/(.*)': '<rootDir>/src/app/core/$1',
  },
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
  testMatch: ['<rootDir>/src/**/*.spec.ts'],
  collectCoverage: true,
  coverageReporters: [
    'text-summary',
    [
      'lcovonly',
      {
        file: 'coverage.lcov',
        includeAllSources: true,
      },
    ],
  ],
  coverageDirectory: 'coverage',
};
